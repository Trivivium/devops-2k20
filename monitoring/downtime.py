"""
calculates the downtime, average recovery time, and average
response time from the log.csv file
"""
import csv
import datetime
import numpy as np


def main():
    get_downtime()


def get_downtime():
    start = None
    end = None
    prev = True

    downtimes = []
    response_times = []

    with open("log.csv", "r") as logfile:
        reader = csv.DictReader(logfile)

        for record in reader:
            if start == None or start > str_to_date(record["timestamp"]):
                start = str_to_date(record["timestamp"])
            if end == None or end < str_to_date(record["timestamp"]):
                end = str_to_date(record["timestamp"])

            try:
                if record["status"] is "0" and prev == True:
                    down = str_to_date(record["timestamp"])
                    prev = False
                elif record["status"] is "1" and prev == False:
                    up = str_to_date(record["timestamp"])
                    downtimes.append((up - down).total_seconds())
                    prev = True

                    response_times.append(float(record["response_time"]))
                elif record["status"] is "1":
                    response_times.append(float(record["response_time"]))

            except Exception as e:
                print(str(e))

    if len(downtimes) == 0:
        total_uptime = 1
        average_recovery_time = 0
    else:
        average_recovery_time = np.mean(downtimes)
        total_uptime = 1 - np.sum(downtimes) / ((end - start).total_seconds())

    average_response_time = np.mean(response_times)

    print(total_uptime, average_recovery_time, average_response_time)
    return (total_uptime, average_response_time, average_recovery_time)


def str_to_date(timestamp):
    return datetime.datetime.strptime(timestamp, "%Y-%m-%d %H:%M:%S")


if __name__ == "__main__":
    main()
