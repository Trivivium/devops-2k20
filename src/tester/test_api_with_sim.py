#!/usr/bin/env python3

import json
import base64
import unittest
import requests
import random
import time


#DOMAIN = "http://127.0.0.1:11501/"
DOMAIN = "http://webapplication:80/"
BASE_URL = f"{DOMAIN}api"
USERNAME = "simulator"
PWD = "super_safe!"
CREDENTIALS = ":".join([USERNAME, PWD]).encode("ascii")
ENCODED_CREDENTIALS = base64.b64encode(CREDENTIALS).decode()
HEADERS = {
    "Connection": "close",
    "Content-Type": "application/json",
    f"Authorization": f"Basic {ENCODED_CREDENTIALS}",
}

L = 0


def get_latest():
    global L
    L += 1
    return L


def is_latest(expected_value: int) -> bool:
    response = requests.get(f"{BASE_URL}/latest", headers=HEADERS)
    return response.json()["latest"] == expected_value


def create_user(username: str) -> requests.Response:
    data = {"username": username, "email": f"{username}@minitwat.dk", "pwd": PWD}
    latest = get_latest()
    params = {"latest": latest}
    response = requests.post(
        f"{BASE_URL}/register", data=json.dumps(data), headers=HEADERS, params=params,
    )
    assert response.ok, f"could not create {username} ({response.text})"
    assert is_latest(latest)
    return response


class TestStringMethods(unittest.TestCase):
    def test_usage_flow(self):
        a = hex(hash(random.random()))
        create_user(a)
        assert is_latest(1)

        data = {"content": "Blub!"}
        url = f"{BASE_URL}/msgs/{a}"
        l = get_latest()
        params = {"latest": l}
        response = requests.post(
            url, data=json.dumps(data), headers=HEADERS, params=params
        )
        assert response.ok, response.text
        assert is_latest(l)

        l = get_latest()
        query = {"no": 20, "latest": l}
        url = f"{BASE_URL}/msgs/"
        response = requests.get(url, headers=HEADERS, params=query)
        assert response.ok, response.text

        assert any(
            msg["content"] == "Blub!" and msg["user"] == a for msg in response.json()
        ), f"`Blub!` not in {response.json()}"
        assert is_latest(l)

        # Test both endpoints
        l = get_latest()
        query = {"no": 20, "latest": l}
        url = f"{BASE_URL}/msgs/{a}"
        response = requests.get(url, headers=HEADERS, params=query)
        assert response.ok, response.text

        assert any(
            msg["content"] == "Blub!" and msg["user"] == a for msg in response.json()
        ), f"`Blub!` not in {response.json()}"
        assert is_latest(l)

        b = hex(hash(random.random()))
        create_user(b)
        c = hex(hash(random.random()))
        create_user(c)

        url = f"{BASE_URL}/fllws/{a}"
        data = {"follow": b}
        l = get_latest()
        params = {"latest": l}
        response = requests.post(
            url, data=json.dumps(data), headers=HEADERS, params=params
        )
        assert is_latest(l)
        assert response.ok, response.text

        data = {"follow": c}
        l = get_latest()
        params = {"latest": l}
        response = requests.post(
            url, data=json.dumps(data), headers=HEADERS, params=params
        )
        assert is_latest(l)
        assert response.ok, response.text

        l = get_latest()

        query = {"no": 20, "latest": l}
        response = requests.get(url, headers=HEADERS, params=query)
        assert response.ok, response.text

        json_data = response.json()
        assert b in json_data["follows"], json_data
        assert c in json_data["follows"], json_data
        assert is_latest(l)

        #  first send unfollow command
        data = {"unfollow": b}

        l = get_latest()
        params = {"latest": l}
        response = requests.post(
            url, data=json.dumps(data), headers=HEADERS, params=params
        )
        assert response.ok, response.text
        assert is_latest(l)

        l = get_latest()
        # then verify that b is no longer in follows list
        query = {"no": 20, "latest": l}
        response = requests.get(url, params=query, headers=HEADERS)
        assert response.ok, response.text
        json_data = response.json()
        assert b not in json_data["follows"], json_data
        assert c in json_data["follows"], json_data
        assert is_latest(l)


if __name__ == "__main__":
    unittest.main()
