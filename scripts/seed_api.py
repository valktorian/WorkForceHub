#!/usr/bin/env python3

from __future__ import annotations

import hashlib
import json
import os
import sys
import uuid
from dataclasses import dataclass
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


DEPARTMENTS = ["Human Resources", "Finance", "Operations", "Engineering", "Sales"]
JOB_TITLES = ["HR Specialist", "People Partner", "Payroll Analyst", "Operations Coordinator", "Talent Associate"]
FIRST_NAMES = ["Adam", "Nora", "Yasmine", "Ilyas", "Sara", "Mehdi", "Lina", "Omar", "Mina", "Sami"]
LAST_NAMES = ["Amrani", "Bennani", "El Idrissi", "Fassi", "Lahlou", "Mansouri", "Rami", "Tazi", "Ziani", "Kabbaj"]
CLEANUP_TASKS: list[dict[str, Any]] = []


@dataclass
class ApiResponse:
    ok: bool
    status: int
    body: dict[str, Any] | list[Any] | None
    text: str
    request_url: str = ""


def main() -> int:
    print(f"Seeding {SEED_RUNS} account/profile pair(s) against {BASE_URL}")
    print(f"Seed namespace: {SEED_NAMESPACE}{' (random mode)' if RANDOM_MODE else ''}")
    print(f"Cleanup after run: {'yes' if CLEANUP_AFTER else 'no'}")

    assert_ok("OpenAPI document", lambda: request("GET", "/gateway-docs/v1/openapi.json"))

    for index in range(1, SEED_RUNS + 1):
        seed_employee(index)

    if CLEANUP_AFTER:
        cleanup_created_records()

    print("Seed run completed.")
    return 0


def seed_employee(index: int) -> None:
    suffix = uuid.uuid4().hex[:8] if RANDOM_MODE else f"{SEED_NAMESPACE}-{index:02d}"
    person = build_person(index, suffix)

    created_account = create_account(person)
    if created_account.get("status") == "created":
        print(f"Created account {person['email']}")
    else:
        print(f"Account already exists {person['email']}")

    login = request("POST", "/api/auth/login", body={
        "email": person["email"],
        "password": SEED_PASSWORD,
    })

    if not login.ok:
        raise RuntimeError(f"Could not login seeded account {person['email']}: {login.status} {login.text}")

    login_data = response_data(login)
    token = login_data.get("accessToken")
    account_id = login_data.get("accountId") or created_account.get("accountId")

    if not token or not account_id:
        raise RuntimeError(f"Login response for {person['email']} did not include token/accountId.")

    if index == 1:
        assert_ok("holiday reference data", lambda: request("GET", "/api/holidays", token=token, query={"year": "2026", "country": "MA"}))
        assert_ok("leave type reference data", lambda: request("GET", "/api/leave-types", token=token))

    created_profile = create_profile(person, account_id, token)
    if created_profile.get("status") == "created":
        print(f"Created profile {person['employeeNumber']}")
    else:
        print(f"Profile already exists {person['employeeNumber']}")

    profile_id = created_profile.get("profileId") or find_profile_id_by_account(account_id, token)
    if not profile_id:
        raise RuntimeError(f"Could not resolve profile id for account {account_id}.")

    seed_domain_records(person, profile_id, token)


def create_account(person: dict[str, Any]) -> dict[str, Any]:
    response = request("POST", "/api/accounts", body={
        "email": person["email"],
        "password": SEED_PASSWORD,
        "firstName": person["firstName"],
        "lastName": person["lastName"],
        "role": "HRAdmin",
    })

    if response.ok:
        return {
            "status": "created",
            "accountId": response_data(response).get("accountId"),
        }

    if response.status == 409:
        return {"status": "exists"}

    raise RuntimeError(f"Could not create account {person['email']}: {response.status} {response.text}")


def create_profile(person: dict[str, Any], account_id: str, token: str) -> dict[str, str]:
    response = request("POST", "/api/profiles", token=token, body={
        "employeeNumber": person["employeeNumber"],
        "firstName": person["firstName"],
        "lastName": person["lastName"],
        "workEmail": person["workEmail"],
        "personalEmail": person["email"],
        "phoneNumber": person["phoneNumber"],
        "address": person["address"],
        "dateOfBirth": person["dateOfBirth"],
        "jobTitle": person["jobTitle"],
        "department": person["department"],
        "managerProfileId": None,
        "employmentType": "FullTime",
        "hireDate": person["hireDate"],
        "organizationRole": person["jobTitle"],
        "employmentStatus": "Active",
        "accountId": account_id,
    })

    if response.ok:
        return {
            "status": "created",
            "profileId": response_data(response).get("id"),
        }

    if response.status == 409:
        return {"status": "exists"}

    raise RuntimeError(f"Could not create profile {person['employeeNumber']}: {response.status} {response.text}")


def find_profile_id_by_account(account_id: str, token: str) -> str | None:
    response = request("GET", f"/api/profiles/by-account/{account_id}", token=token)
    if not response.ok:
        return None

    profile = response_data(response)
    return profile.get("id")


def seed_domain_records(person: dict[str, Any], employee_id: str, token: str) -> None:
    marker = f"seed:{person['seedSuffix']}"
    seed_time_records(person, employee_id, token, marker)
    seed_evolution_records(person, employee_id, token, marker)


def seed_time_records(person: dict[str, Any], employee_id: str, token: str, marker: str) -> None:
    work_date = person["seedWorkDate"]
    period_start = person["seedPeriodStart"]
    period_end = person["seedPeriodEnd"]
    leave_start = person["seedLeaveStart"]
    leave_end = person["seedLeaveEnd"]

    entries = read_items("GET", f"/api/time-entries/by-employee/{employee_id}", token, query={"from": work_date, "to": work_date})
    ensure_record(
        "time entry",
        any(item.get("notes") == marker for item in entries),
        lambda: request("POST", "/api/time-entries", token=token, body={
            "employeeId": employee_id,
            "workDate": work_date,
            "startTime": "09:00:00",
            "endTime": "17:00:00",
            "projectCode": "SEED",
            "taskCode": person["seedSuffix"].upper(),
            "notes": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("time entry", "DELETE", f"/api/time-entries/{item_id}", token),
    )

    timesheets = read_items("GET", f"/api/timesheets/by-employee/{employee_id}", token, query={"periodStart": period_start, "periodEnd": period_end})
    ensure_record(
        "timesheet",
        any(same_date(item.get("periodStart"), period_start) and same_date(item.get("periodEnd"), period_end) for item in timesheets),
        lambda: request("POST", "/api/timesheets", token=token, body={
            "employeeId": employee_id,
            "periodStart": period_start,
            "periodEnd": period_end,
        }),
    )

    leave_requests = read_items("GET", f"/api/leave-requests/by-employee/{employee_id}", token)
    ensure_record(
        "leave request",
        any(item.get("reason") == marker for item in leave_requests),
        lambda: request("POST", "/api/leave-requests", token=token, body={
            "employeeId": employee_id,
            "leaveType": "Annual",
            "startDate": leave_start,
            "endDate": leave_end,
            "reason": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("leave request", "POST", f"/api/leave-requests/{item_id}/cancel", token, {"comment": "Seed cleanup."}),
    )

    balances = read_items("GET", f"/api/leave-balances/{employee_id}", token)
    ensure_record(
        "leave balance",
        any(item.get("leaveType") == "Annual" for item in balances),
        lambda: request("POST", f"/api/leave-balances/{employee_id}/adjust", token=token, body={
            "employeeId": employee_id,
            "leaveType": "Annual",
            "delta": 18,
            "reason": marker,
        }),
    )


def seed_evolution_records(person: dict[str, Any], employee_id: str, token: str, marker: str) -> None:
    effective_date = person["seedEffectiveDate"]

    job_movements = read_items("GET", f"/api/job-movements/employee/{employee_id}", token)
    ensure_record(
        "job movement",
        any(item.get("comment") == marker for item in job_movements),
        lambda: request("POST", "/api/job-movements", token=token, body={
            "employeeId": employee_id,
            "previousJobTitle": "Associate",
            "newJobTitle": person["jobTitle"],
            "previousDepartment": "Operations",
            "newDepartment": person["department"],
            "effectiveDate": effective_date,
            "reason": "Seeded career movement",
            "comment": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("job movement", "DELETE", f"/api/job-movements/{item_id}", token),
    )

    salary_changes = read_items("GET", f"/api/salary-changes/employee/{employee_id}", token)
    ensure_record(
        "salary change",
        any(item.get("comment") == marker for item in salary_changes),
        lambda: request("POST", "/api/salary-changes", token=token, body={
            "employeeId": employee_id,
            "previousSalary": 9000 + person["seedIndex"] * 100,
            "newSalary": 10500 + person["seedIndex"] * 100,
            "currency": "MAD",
            "effectiveDate": effective_date,
            "reason": "Seeded compensation change",
            "comment": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("salary change", "DELETE", f"/api/salary-changes/{item_id}", token),
    )

    trainings = read_items("GET", f"/api/trainings/employee/{employee_id}", token)
    ensure_record(
        "training",
        any(item.get("comment") == marker for item in trainings),
        lambda: request("POST", "/api/trainings", token=token, body={
            "employeeId": employee_id,
            "title": f"Seed Onboarding {person['seedSuffix'].upper()}",
            "provider": "WorkForceHub Academy",
            "status": "Completed",
            "startDate": person["seedTrainingStart"],
            "endDate": person["seedTrainingEnd"],
            "completionDate": person["seedTrainingEnd"],
            "certificateUrl": None,
            "comment": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("training", "DELETE", f"/api/trainings/{item_id}", token),
    )

    rewards = read_items("GET", f"/api/rewards/employee/{employee_id}", token)
    ensure_record(
        "reward",
        any(item.get("comment") == marker for item in rewards),
        lambda: request("POST", "/api/rewards", token=token, body={
            "employeeId": employee_id,
            "title": f"Seed Recognition {person['seedSuffix'].upper()}",
            "type": "Bonus",
            "value": 750 + person["seedIndex"] * 10,
            "grantedAt": person["seedRewardDate"],
            "reason": "Seeded recognition",
            "comment": marker,
        }),
        cleanup=lambda item_id: queue_cleanup("reward", "DELETE", f"/api/rewards/{item_id}", token),
    )


def build_person(index: int, suffix: str) -> dict[str, Any]:
    seed = stable_seed(suffix)
    first_name = pick(FIRST_NAMES, seed)
    last_name = pick(LAST_NAMES, seed >> 3)
    department = pick(DEPARTMENTS, seed >> 5)
    job_title = pick(JOB_TITLES, seed >> 7)
    safe_suffix = suffix.lower()

    return {
        "seedIndex": index,
        "seedSuffix": safe_suffix,
        "firstName": first_name,
        "lastName": last_name,
        "department": department,
        "jobTitle": job_title,
        "email": f"seed.account.{safe_suffix}@workforcehub.local",
        "workEmail": f"seed.profile.{safe_suffix}@workforcehub.local",
        "employeeNumber": f"SEED-{safe_suffix.upper()}",
        "phoneNumber": f"+21260000{index:04d}",
        "address": f"{department} Seed Office, Casablanca",
        "dateOfBirth": f"{1988 + (index % 12)}-03-10T00:00:00Z",
        "hireDate": f"2026-{((index % 9) + 1):02d}-01T00:00:00Z",
        "seedWorkDate": f"2026-04-{index:02d}",
        "seedPeriodStart": "2026-04-01",
        "seedPeriodEnd": "2026-04-30",
        "seedLeaveStart": f"2026-05-{index:02d}",
        "seedLeaveEnd": f"2026-05-{index:02d}",
        "seedEffectiveDate": f"2026-06-{index:02d}T00:00:00Z",
        "seedTrainingStart": f"2026-02-{index:02d}T00:00:00Z",
        "seedTrainingEnd": f"2026-02-{index + 1:02d}T00:00:00Z",
        "seedRewardDate": f"2026-07-{index:02d}T00:00:00Z",
    }


def assert_ok(name: str, action: Any) -> None:
    response = action()
    if not response.ok:
        raise RuntimeError(f"{name} failed: {response.status} {response.text}")

    print(f"{name} OK")


def ensure_record(name: str, exists: bool, create: Any, cleanup: Any | None = None) -> None:
    if exists:
        print(f"{name} already exists")
        return

    response = create()
    if response.ok:
        response_value = response_data(response)
        if "already exists" in str(response_value.get("message", "")).lower():
            print(f"{name} already exists")
            return

        print(f"Created {name}")
        created_id = response_value.get("id")
        if cleanup is not None and created_id:
            cleanup(created_id)
        return

    if response.status == 409:
        print(f"{name} already exists")
        return

    raise RuntimeError(f"Could not create {name}: {response.status} {response.request_url} {response.text}")


def queue_cleanup(name: str, method: str, path: str, token: str, body: dict[str, Any] | None = None) -> None:
    CLEANUP_TASKS.append({
        "name": name,
        "method": method,
        "path": path,
        "token": token,
        "body": body,
    })


def cleanup_created_records() -> None:
    if not CLEANUP_TASKS:
        print("No created cleanup records.")
        return

    print(f"Cleaning {len(CLEANUP_TASKS)} created record(s).")
    for task in reversed(CLEANUP_TASKS):
        response = request(task["method"], task["path"], token=task["token"], body=task["body"])
        if response.ok or response.status in (204, 404, 409):
            print(f"Cleaned {task['name']}")
            continue

        raise RuntimeError(f"Could not clean {task['name']}: {response.status} {response.request_url} {response.text}")


def read_items(method: str, path: str, token: str, query: dict[str, str] | None = None) -> list[dict[str, Any]]:
    response = request(method, path, token=token, query=query)
    if response.status == 404:
        return []

    if not response.ok:
        raise RuntimeError(f"Could not read {path}: {response.status} {response.text}")

    data = response_data(response)
    if isinstance(data, list):
        return data

    if isinstance(data, dict) and isinstance(data.get("items"), list):
        return data["items"]

    return []


def same_date(value: Any, expected: str) -> bool:
    return isinstance(value, str) and value[:10] == expected


def request(method: str, path: str, body: dict[str, Any] | None = None, token: str | None = None, query: dict[str, str] | None = None) -> ApiResponse:
    url = f"{BASE_URL}{path}"
    if query:
        url = f"{url}?{urlencode(query)}"

    headers = {"Accept": "application/json"}
    data = None

    if body is not None:
        data = json.dumps(body).encode("utf-8")
        headers["Content-Type"] = "application/json"

    if token:
        headers["Authorization"] = f"Bearer {token}"

    req = Request(url, data=data, headers=headers, method=method)

    try:
        with urlopen(req, timeout=30) as response:
            text = response.read().decode("utf-8")
            return ApiResponse(True, response.status, parse_json(text), text, url)
    except HTTPError as exc:
        text = exc.read().decode("utf-8")
        return ApiResponse(False, exc.code, parse_json(text), text, url)
    except URLError as exc:
        raise RuntimeError(f"Could not reach {url}: {exc}") from exc


def response_data(response: ApiResponse) -> dict[str, Any]:
    if isinstance(response.body, dict) and isinstance(response.body.get("data"), dict):
        return response.body["data"]

    if isinstance(response.body, dict):
        return response.body

    return {}


def parse_json(text: str) -> dict[str, Any] | list[Any] | None:
    if not text:
        return None

    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return None


def normalize_token(value: str) -> str:
    normalized = []
    previous_dash = False
    for char in value.lower():
        if char.isalnum():
            normalized.append(char)
            previous_dash = False
        elif not previous_dash:
            normalized.append("-")
            previous_dash = True

    return "".join(normalized).strip("-")


def stable_seed(value: str) -> int:
    digest = hashlib.sha256(value.encode("utf-8")).hexdigest()
    return int(digest[:12], 16)


def pick(items: list[str], seed: int) -> str:
    return items[abs(seed) % len(items)]


BASE_URL = (os.getenv("WORKFORCEHUB_API_BASE_URL") or os.getenv("API_BASE_URL") or "http://localhost:5000").rstrip("/")
SEED_RUNS = int(os.getenv("SEED_RUNS") or "10")
SEED_NAMESPACE = normalize_token(os.getenv("SEED_NAMESPACE") or "demo-v1")
SEED_PASSWORD = os.getenv("SEED_ACCOUNT_PASSWORD") or "CHANGE_ME_DEMO_PASSWORD"
RANDOM_MODE = (os.getenv("SEED_RANDOM") or "").lower() == "true"
CLEANUP_AFTER = (os.getenv("SEED_CLEANUP_AFTER") or "false").lower() == "true"


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Seed run failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
