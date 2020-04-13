"""
Creates a `log.csv` file where monitoring history is appended and saved.
"""
import requests
import os
import csv
import time
import datetime
import sys


def main():
    WEBPAGE = "http://minitwit.dk/"

    if not log_exists():
        create_log()
        print("created log")
    monitor(WEBPAGE)


def monitor(webpage):
    while True:
        try:
            response = requests.get(webpage)
            print(int(response.status_code))
            print(response.elapsed.total_seconds())
            if response.status_code == requests.codes.ok:
                response_time = response.elapsed.total_seconds()
                print("alive")
                append_to_file(1, response_time)
            else:
                print("dead")
                append_to_file(0)
            time.sleep(1)
        except Exception as e:
            print("no internet")
            print(str(e))


def current_timestamp():
    return datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")


def str_to_date(timestamp):
    return datetime.datetime.strptime(timestamp, "%Y-%m-%d %H:%M:%S")


def log_exists():
    return os.path.isfile("log.csv")


def create_log():
    with open("log.csv", "a") as log_file:
        columns = ["timestamp", "status", "response_time"]
        writer = csv.DictWriter(log_file, fieldnames=columns)
        writer.writeheader()


def append_to_file(status, response_time=1000):
    with open("log.csv", "a") as log_file:
        columns = ["timestamp", "status", "response_time"]
        writer = csv.DictWriter(log_file, fieldnames=columns)
        writer.writerow(
            {
                "timestamp": str(current_timestamp()),
                "status": status,
                "response_time": response_time,
            }
        )


if __name__ == "__main__":
    main()
