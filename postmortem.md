# Security Review Postmortem
This document will outline a [Postmortem
documentation](https://en.wikipedia.org/wiki/Postmortem_documentation "wikipedia
explaination") of our monitoring and pen-testing of the `U buntu?` service,
developed by *Group C*, as well as our security review of our own service.

## Review of Group C
We started by creating a monitoring tool collecting data about the `U buntu?`
service, and then started pen-testing their service. After approximately a week
of gathering monitoring data we had enough information to confidently evaluate
whether Group C were violating their Service-Level agreement. The following
order is therefore not a reflection of the order in which the tasks were
done, but merely with the intent of readability.

### Creating a monitoring tool
To investigate whether or not Group C lives up to their service-level agreement, we created a python script to monitor the uptime, response-time, and recovery-time of their webpage. The python script creates a csv file where metrics are continuously appended to the file. This is facilitated by the requests library that helps us send get requests to their webpage. If we receive a 200 status code, we log that the webpage is running as intended as well as a timestamp and the elapsed time since the request was sent. To make sure that this script is running 24/7, we moved the script to our droplet running our minitwit and run the script with a nohup command.

### Evaluating the monitored data
From the metrics in the csv file, we can calculate the requirements dictated by Group C’s SLA. This calculation is done in a separate python script and outputs the following results:

Uptime: 100%

Response Time: 1.87 seconds

Time to Recover: 0 seconds

We see that the uptime of the webpage and therefore also the recovery time, is flawless and therefore lives up to the SLA. The response time, however, is considerably slower than the 200ms specified by the SLA. We gave the webpage the benefit of doubt and assumed that our monitoring implementation was somehow incorrect, but cross-validation with 3rd-party tools such as dotcom-tools.com confirmed that this was the case. 


### Pen testing `U Buntu?`
PLEASE ADD SOME SHIT HERE **ADAM**


## Security review of own service
PLEASE ADD SOME SHIT HERE **ANDERS**
