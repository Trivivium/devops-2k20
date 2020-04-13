# Monitoring
These two python files can be run from a server to check up on the service of
**Group C**, and then analyze the results from this. 

### Responsibilities

* `monitor.py` creates a `log.csv` file where monitor stats are appended. 
* `downtime.py` calculates the downtime, average recovery time, and average
  response time from the log.csv file 
