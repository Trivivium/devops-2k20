# Security Review Postmortem
This document will outline a [Postmortem
documentation](https://en.wikipedia.org/wiki/Postmortem_documentation "wikipedia
explaination") of our monitoring and pen-testing of the `U buntu?` service,
developed by *Group C*, as well as our security review of our own service.

## Review of Group C
We started by creating a monitoring tool collecting data about the `U buntu?`
service, and then started pen-testing their service. After approximately a week
of gathering monitoring data we had enough information to confidently evaluate
whether Group C were violating their Service-Level Agreement(SLA). The following
order is therefore not a reflection of the order in which the tasks were
done, but merely with the intent of readability.

### Creating a monitoring tool
To investigate whether or not Group C lives up to their service-level agreement,
which had the following promises:

- *Up-time:* 99%
- *Average Response Time:* 200ms
- *Time To Recover:* 2 hours

To validate this we created two scripts, one for collecting data, done by
monitoring their service, and one for evaluating the collected data.

We chose to develop these tools in python3.6, as we would be able to run the
script from anywhere, while still developing a usable program in a short timeframe.

#### Monitoring script
The monitoring script continuously appended data to a `csv`-file. This is
facilitated by the requests library that helps us send `GET` requests to their
webpage. We then logged the response-code from their service. A 200 status code,
we log that the webpage is running as intended as well as a timestamp and the
elapsed time since the request was sent, if no response was returned or a
non-200 was returned, we would mark it as a failure. We pushed this script to
our droplet so we would be able to run it without interruption for several days.

The script can be found
[here](https://github.com/Trivivium/devops-2k20/blob/master/monitoring/monitor.py). 

#### Evaluation script
Our secondary script takes the `csv`-file as input, and then calculates the
up-time, average response time, and the time it had taken to recover if any
downtime had existed. Running this script at any point in time would reveal this
information.

This script is simpler in nature, and can be found
[here](https://github.com/Trivivium/devops-2k20/blob/master/monitoring/downtime.py).


### Evaluating the monitored data
After running the monitoring script for a couple of days, we were able to
extract the following results:

Uptime: 100%

Average Response Time: 1.87 seconds

Time to Recover: 0 seconds

We see that the uptime of the webpage and therefore also the recovery time, is
flawless and therefore lives up to the SLA. The response time, however, is
considerably slower than the 200ms specified by the SLA. We initially feared
that it was simply our script that had an overhead, or the network time to our
droplet, however after cross-validation with 3rd-party tools such as
dotcom-tools.com, we could confirm that their response time was in fact slower
than promised. 

Therefore we could prove that they had violated their SLA. 


### Pen testing `U Buntu?`
PLEASE ADD SOME SHIT HERE **ADAM**


## Security review of own service

### Define Assets 

**Our assets are:**

Source Code:
If source code contains any secrets such as passwords and the like.
Information how the system is built and so forth.

Access to the Database:
If access is gained to the database it is possible to administer the database,
giving you full access to the database users and all data contained within the
database.

Access to audit data: 
The audit data shows all auditable events that occurred within the application.

**Vulnerabilities:**

- Open ports


**Threats:**

- Denial of service
- ClickJacking


### Risk Assessment Matrix


|          | Negligible | Marginal          | Critical     | Catastrophic |
|----------|------------|-------------------|--------------|--------------|
| Certain  |            | Denial of service |              |              |
| Likely   |            |                   |              |              |
| Possible |            |                   |              | DB           |
| Unlikely |            |                   | ClickJacking |              |
| Rare     |            |                   |              |              |



## Penetration Testing of our own system

Our approach is to do both a penetration test, and a security audit. 

Tools: 
- Kali Linux
- Nmap 
- SqlMap
- Metasploit
- OWASP ZAP

**Procedure**:

In the security audit, we tried to follow OWASP guidelines for developing a
secure application [Application Security Verification
Standard](https://owasp-aasvs.readthedocs.io/en/latest/).
One of the points is to [Verify that secrets, API keys, and passwords are not
included in the source code, or online source code
repositories](https://owasp-aasvs.readthedocs.io/en/latest/requirement-2.29.html).
We realized that we are storing several passwords in our source code. The admin
password to the database can be found in both  our docker-compose file, as well
as our sourcecode. Additionally passwords to our admin user is visible in the
sourcecode, as well as passwords to our logging tools. 
This would in theory be acceptable if these can be changed in deployment, and
regarding the admin user password, if it is possible to change via the user
interface. This, however, is not the case.
That is a huge security flaw and must be solved.

The solution to this would be to implement a change-password functionality as
well as store all external authorization credentials via environmental
constants.

Working with docker containers we fixed it by using [environment variables](https://docs.docker.com/compose/environment-variables/):
E.g in the docker-compose file
```
environment:
- PW

```
and then it is possible to pass the variable from the terminal to the container

````
docker-compose run -e PW=UiuiUi

````


Next is the penetration test in which we have divided into several steps.


- **Step 1**
The first step is to make a port scan of the system. We use Nmap to get an
overview of Enable OS detection and version detection and open ports.
```
nmap -v -A 46.101.119.181
```

The most worrying finding is the open port at 1433 because it is our database.
Also, in continuation of our admin database password being available in source
code makes it quite straightforward to access our database.
 
 - **Step 2**

Firstly, we will try to test if we are vulnerable to Injection flaws, such as
SQL injection. SQL injection is the highest security risk according to OWASP and
in that case, it makes sense to try that on our system. we used SqlMap to figure
out if any of our inputs are injectable. If not, then SqlMap will return -
*[WARNING] heuristic (basic) test shows that POST parameter 'username' might not
be injectable* That is, our system is not vulnerable regarding SQL injection.
All tested parameters do not appear to be injectable. 
 
 - **Step 3** 
Information about the DB.

Because the Database port is open it is possible to acquire information about
the database. We will use a tool from Metasploit that will enumerate MSSQL
configuration setting. 

In essence, the module will perform a series of configuration audits and
security checks against our Microsoft SQL Server database. 

The password is required for the module to work and because the password is
accessible in our source code it is not difficult to acquire.

Using auxiliary/admin/mssql/mssql_sql will allow for simple SQL statements to be
executed against an MSSQL instance given appropriate credentials. That is, if
someone knows our password then they had all they did need to delete our
database. Also, because we have stated it would be catastrophic in our risk
matrix if something happens to our database.

- **Step 4**

We have also tried to use another tool called OWASP ZAP.

It is an open-source web application security scanner and its main goal is to
find vulnerabilities in web applications. It will run different attack scenarios
against the web application and record the results. 

The results that we got from OWASP ZAP were:
```
X-Frame-Options Header Not Set

Risk: Medium

X-Frame-Options header is not included in the HTTP response to protect against 'ClickJacking' attacks.
```
and

```
X-Content-Type-Options Header Missing

Risk: Low

The Anti-MIME-Sniffing header X-Content-Type-Options was not set to 'nosniff'. This allows older versions of Internet Explorer and Chrome to perform MIME-sniffing on the response body, 
potentially causing the response body to be interpreted

```

A comment on the first result is that it can be solved by setting the
X-Frame-Options HTTP header and ensuring it is set on all web pages returned by
our web application.

A comment on the second result  is that it can be solved by ensuring that the
web application sets the Content-Type header appropriately and that it sets the
X-Content-Type-Options. Ensure that the application/web server sets the
Content-Type header appropriately and that it sets the X-Content-Type-Options
header to 'nosniff' for all web pages. Overall, OWASP ZAP did not find any
**critical** vulnerabilities and both vulnerabilities that OWASP ZAP reported
can be solved with few lines of code in our application. It is all about setting
headers correctly concerning security.
