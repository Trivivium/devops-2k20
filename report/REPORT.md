# Development Report
This document will detail the process of migrating the Minitwit platform, as well as the 
*TODO - improve introduction*

## System Description
*TODO - design and architecture of our ITU-MiniTwit system*

## Application Solution details
*TODO - dependencies and important interactions of subsystems*

### Hosting
As the application should be publically available IP address we needed a hosting
provider for the production environment. The course presented Digital Ocean as an
option, and as a couple of us had prior experience with them we choose this solution.
The prior experience along with the option to utilize the Github Education Pack for
free credits meant we had confidence in the decision.

We started out provisioning a small droplet, which is what Digital Ocean names
their virtual private servers (VPS), with enough resources to host the application
and the database inside Docker containers. However, whenwe added monitoring
and logging to application we had increasing requirements for the specs of the VPS. 
To meet these requirements we provisioned multiple droplets as scaling a single VPS vertically would introduce downtime, which we wanted to avoid. 

The result of this approach led us to having two droplets; one for our tools (e.g.,
logging) and one for the solution. In retrospect this was probably the right
decision as it increases our resiliency. If one of the droplets crashes the other
one remains untouched.

Despite the short-term benefit of this solution we still have issues scaling the
droplet hosting the application. Because the application isn't stateless horizontal
scaling isn't an option, and thus vertical scaling is our sole option, which requires
us to incur some downtime when the droplet is upgraded.

### Operating system
For the operating system of the application droplet we decided on Ubuntu 18.04.3 LTS.
It was important for us to use a version with long-term support (LTS) as it helps
us ensure stability and reliability as well as active support should bugs or 
security vulnerabilties surfaced during the course. 

We wanted to use a Linux based distribution as it seemed to provide the greatest
level of learning. Using Windows based virtual machine could provide us a graphical
user interface and tools, and it would definitely yield some valuable learning as well
it seemed a less attractive choice in the context of this course and tools we aimned
to utilize. The choice of Ubuntu is definitive as we could have used Arch Linux or 
any other distribution. However, Ubuntu is rather common and has a great community
making tools, which make it easier to figure out how tasks are done. These considerations
combined made Ubuntu an ideal choice for the members of the group that didn't have a
extensive experience with the OS and thus a more limited level of proficiency with it.

Due to the technologies we were planning on using (i.e., Docker) we weren't going to
be working too much directly on the operating system level. This meant that we didn't
require to have one of the group members focusing on the OS more than others. It also
proved to be a comfortable environment for the group members used to working in Windows.

### Containerization
To run the application and the database instance required by the application we decided
on the use of Docker. This choice rested primarily on it being presented in the course,
but a more important fact was that all group members had interest in using the 
technology, and a good introduction to the fundamentals around containers, which has 
broad applicability in other technologies such as Kubernetes.

In a professional context it might have been a better choice to use some
containerization-as-a-service solution or a provider where the features underlying
operating system support Docker is managed. Sticking with installing, configurating,
and operating Docker was a lot more involved than a managed solution in terms of
manual work, but it provided us with invaluable learning opportunities.

There are other alternatives to Docker, but it is the primary technology supporting
containerizatio and thus an unoffical standard in the business. An example of an alternative is Vagrant used to provision the servers, but we deemed it less attractive
as it is a rather heavy-weight solution (i.e., entire operating system) in order to gain the same isolation Docker provides.

### Programming language & Runtime environment

**.NET Core 3.1**

*TODO - Write some about this choice and the value we expect from it*

**C# 8.0**
Early on we had to convert the existing system into a new programming language. 
We had a short discussion about programming languages, and it was argued that C#
was the language that most people would be able to write from the get go. We
wanted to focus less on the development of the system, and rather on setting up
DevOps tools and things in that regard, so picking a challenging language was
not prioritized, rather something that would make the code itself somewhat easy
to debug, seemed more sensible.

**Evaluation**
The team probably has varying opinions on the C# language, and some of use would
probably have preferred somewhat that was more engaging or faster to write,
however it got the job done, and it did make it easier to debug, leaving time
for writing various tests.

### Testing
**Unit tests: XUnit (C#)**
To help increase our confidence in the changes added to the system we added
unit tests. The unit-tests are written in C# using the XUnit test framework,
which is used to test functionality as implemented in web application
(e.g., creating a user, adding a message, etc.).

The unit tests are focused around the service classes, which implements the
business logic related to the functionality of the system. These tests aims
to check the "happy-path" where the function succeeds as well as the expec-
ted error paths (i.e., adding a message to an unknown user). The tests are
executed using the built-in tooling of the dotnet CLI included in the .NET
Core SDK.

These tests are run as part of the CI pipeline on pull requests
(PR), and when a PR is merged into the master branch.

**Evaluation**
The unit-tests are quick to run in the CI pipeline. However, the amount of
code currently covered by the tests are limited to the primary functiona-
lity. This is, however, not a reflection of the choice of strategy, but rather the available time we allocated to this part of the project. 

Some tests are without a doubt better than no tests, and we did catch some bad exceptions once in a while. Using the "happy-path" mentality strategy did catch the worst of errors however, meaning that it definitely added value to the project as a whole, however there is naturally always room for improvement. The focus did, however, mean that we tested wide, and meant that most features were covered.
The unit-tests are quick to run in the CI pipeline. However, the amount of
code currently covered by the tests are limited to the primary functiona-
lity. 

**Integration tests: Python 3.8**
We wanted to test the API used by the simulator to make sure that this would
continue to work - Also as this surface is a API it is easier to test than a
graphical user interface, so we would be able to test all backend functionality
relatively easy.

The simulator that had to interface with our service was already written in
python3, so testing if the simulator would be able to work, would, in theory, be
as simple as running the simulator on a clean database, and see if the simulator
failed. It would still have to be rewritten a bit though

As a member of our team has worked with python professionally for multiple years
he proposed he would convert it into unit tests. This made it somewhat easy to
isolate where errors occurred if something failed within this tester.

Python being fast to prototype in and great for scripts, also made this an ideal
choice for blackbox testing. 

**Evaluation**
This has so far proven to be a great choice. We haven't had to modify the files
yet, so the primary factor seemed to be development time, which was low, so the
choice seemed perfect.

### Database
**Microsoft SQL Server**
We did not want to use SQLite in production, so we had to change to another
database scheme. Due to already picking C# for the programming language, it
seemed to make sense to continue the development in the same technology sphere,
by picking yet another microsoft product. This also provided the best possible
object relation mapping (ORM), which made development somewhat easier.

**Evaluation**
The only problem we've had with this, was that a subset of our development team
continued to use SQLite for local testing, and SQLite has a more relaxed
relation to constraint, so some errors would occure in production that didn't
locally. However we fixed this by making it easier to spin up a MSSQL database
locally. 
However MSSQL in itself provided no problems - it had exactly the features that
we were after, and worked like a charm. This seemed to have been a good choice.

## State of solution
*TODO - current state of our system, results of static analysis and code quality assessment, add security assessment too*

## CI/CD implementation
*TODO - complete description of stages and tools used in CI/CD chains (deployment and release)*

**Continuous Integration: Github Actions**
We initially looked at a variety of different CI possibilities, and considered
Jenkins for one, however we randomly looked at github and a group member
asked if any of us have every tried using Github Actions, their CI solution, and
we realized that neither of us had. This seemed like a great way to try
something new and it also seemed ideal with the current stack we were running. 
The choice was mainly based on this: the availability and the possibility to
learn something new. 

**Evaluation**
It was really neat having it integrate so smoothly with our pull request flow.

**Packaging & Docker image storage: Github**
Github has the possibility to host and provide packages for downloading, much
like Dockerhub. We considered moving our images to Dockerhub, but having in
centralized one place seemed ideal. We liked the idea of having a few select
services that we relied on, as to not create a too complex development flow, but
rather utilize few tools that integrated nicely together and provided each a
large chunk of the features required.

**Evaluation**
We had a few issues, especially that you [cannot download without being logged
in](https://github.community/t5/GitHub-API-Development-and/Download-from-Github-Package-Registry-without-authentication/td-p/35255),
which was rather tedious, but we overcame it by having one of our personal
tokens stored on the production site (which is less than ideal, but works).
Other than that it provided the features we needed. Looking at the features we
use, we would have gotten nothing more out of using Dockerhub, so having it all
centralized one place, packaging, repository & CI seemed like a neat choice, on
that we stand by.

**Continuous Delivery: Watchtower**
We needed our production server to automatically update whenever the master
branch updated on our repository - which was when ever a new version was
released on the packaging provider. There is a few different tools for this, and
this is something Jenkins does provide, however installing Jenkins for this
feature alone, seemed overkill and would use a magnitude more CPU power than we
wanted. So we found a service called Watchtower, which runs as a docker
container, and with with a consistant interval checks whether the remote image
has been updated - if it has, it updates the running container. 

**Evaluation**
It worked as expected and we didn't really touch it after installing it. This was
lightweight and provided exactly what we needed. Some critique can be made of
the service, as it in theory has access to the whole docker system, which might
be dangerous, and we are not exactly sure how it handles any errors in building
a new package, so it is probably more error prone, however it works for our
limited need. If we were to scale up and use this in a more system critical
setting, we would probably research this more, and find a enterprise grade
solution. Maybe even giving the github actions direct access to the Docker
service, and giving it the possibility to update the service directly from a
Github Action. We didn't do this initially out of fear for opening up our docker
host to the world, as that seemed relatively risky. However, with more research,
we could probably find a way to do this in a secure way.

## Development practices
*TODO - how do we interact? how is the team organized?*

*TODO - organization of our repository, branching strategy, process and tools (how do we use issues, tasks, kanban board)*

**Issue tracking / Kanban: Github**
Generally speaking we never really put much thought into how we would track
issues and how we would separate the tasks at hand. As we already had Github
open, we simply created all our tasks on the issue board there and never thought
about alternatives. 
Alternatively we could have created a Trello board or a Jira project, however
with the limited scope of the project it seemed extensive to include a whole
other system just for task management. As previously mentioned we generally
tried limiting the number of different tools we used, and create a stack with as
few different tools as possible.

**Evaluation**
We definitely had problems with our taskmanagement and ended up doing some of the
tasks too late, so we definitely had to change our workflow, and would have if
we could do it over. I think the main issue was that we didn't consult the issue
list often enough, and possibly didn't put deadlines on, as well as not
assigning people to issues. Ideally we should probably have improved our overall
development process earlier on, but this is covered in the [Post
Mortem](postmortem.md). We probably wouldn't have gotten any alternative
important features by choosing another service, as the problems we had were
based on structural team problems rather than the tool itself. Having the issues
closely aligned with the pull-request flow was definitely a helpful feature.

## Devops tools
*TODO: Write an introduction to this section*

### Monitoring
*TODO - how(and what) do we monitor in our system*

**Monitoring: Prometheus & Grafana**
We chose Prometheus as the primary monitoring tool and Grafana for data visualization.
No one from the team had a lot of experience working with monitoring tools. It meant that no one had expectations or any preference regarding choosing the monitoring tool. 
Taking a look at [Prometheus's comparison to alternatives](https://prometheus.io/docs/introduction/comparison/) (even though it should be taken with a grain of salt as they
made the comparison themself) it made it clear that it did fit into the setup because Prometheus is designed to
monitor targets as in servers, containers and the like.

Also, supporting active monitoring by periodically scrap our application by pulling data from this target.
A pull-based system enables us to rate control in which it will pull the data. 
With a push-based system we may have the risk of sending too much data towards our server and in worst case crash it. 

To set up Prometheus and Grafana is just a matter of creating docker containers. 

**Evaluation**
In our case, it must integrate well into our current setup, and it is well-supported. That is, there is some kind of official
library for the tool that we want to use, and it is actively maintained.
[Prometheus has an official GitHub repository for .NET](https://github.com/prometheus-net/prometheus-net) with examples of getting started which fits with our criteria.
Alternatives like Graphite there is no official GitHub repository, and a [simple search on Github](https://github.com/search?q=graphite+.net) reveals it. It also the fact that if the community is large enough
there is a possibility of finding a solution to your problem in a short amount of time.

### Logging
*TODO - what do we log and how we aggregate it*

**ELK**
We chose Elasticsearch, Logstash and Kibana (the ELK stack) as logging tools.
Firstly, it enables us to do modern, scalable and user-friendly logging, and it is also one of the most popular
choices at the moment.
Because it promotes centralized logging we chose to set up the ELK stack on another droplet, and the fact that
Elasticsearch requires a larger amount of memory.

To setup the ELK stack is just a matter of creating docker containers. 

**Evaluation**
As we prioritize ease of integration of the toolset that we choose, it felt that the ELK stack was the right choice.
We tried to look for alternatives to avoid making the easy choice of choosing the most popular one.
We had a look at [LogDNA](https://logdna.com) that is Elasticsearch and Kibana combined. It also supports containerized 
environments even though they suggest using Kubernetes with their product.
We want to ensure that libraries used for this product are well-supported and maintained for .NET. We found a Git
Repository named [RedBear.LogDNA](https://github.com/RedBearSys/RedBear.LogDNA) where a library resides for connecting to logDNA from .NET. 
Studying the issues that were made did make the choice easy for us as some issues stated that the library leads to system
failure was not something that we wanted to work with. 
Furthermore, it should be simple to acquire help which did not look like to be the case.

### Scaling and load balancing
*TODO- strategy for scaling and load balancing*

**Docker Swarm**
We choose Docker Swarm as the technology used to scale the system. As this step
was required later in the course the choice integrates beautifully with our
prior investment into Docker Compose. To keep the setup simple we decided to
run Docker Swarm with a single node (the original host machine) to act as the 
swarm manager and only worker as it also hosts the web applicaton and database.

We didn't invest too much time looking into alternatives as Docker Swarm seemed
to provide all the tools necessary with less technical fragmentation (i.e., using
several different providers with differing configuration systems).

**Evaluation**
In accordance with your prior interest in Docker the choice of Docker Swarm was a
natural extension of this. The main hurdles encountered is the isolated knowledge 
of the technology. Due to nature of it we allocated a single person to set it up,
which meant that when an error was encountered by others in the group we didn't
have a clear picture. However, this can be helped by documenting the approach
taken and sharing lessons learned.

When considering the choice of using a single node there are some consequences we
are aware of, but which means true scaling and reliability isn't archieved. In the
case that the node crashes is the entire system also taken down. The scaling aspect
is also constrained to the resources available on that single node. Both of these
issues can be resolved by adding more physical machines to the swarm, which we can do seamlessly because of docker swarm. However, this
was a deliberate decision in order to keep monetary costs down.

## Conclusion and evaluation
*TODO - biggest issues, major lessons we have learned, overall takeaways, fuckups etc. regarding:*

**Containerization (Docker) Evaluation**
We would probably prefer having a more powerful host for the containers in the
future. If we had to scale vertically it would presumably be difficult, and this
is handled better in systems like Docker Swarm or Kubernetes. There are a large variety 
of different tools, which still builds on the Docker syntax, which has a more expansive
set of features, that would presumably handle scalability challanges better, however it
worked for the relatively small service that we had to provide - so in this context, it
was probably an ideal choice.

If we didn't want to focus on an approach that would provide us with a good learning
opportunity, we could have picked Azure and focus entirely on the application, as it 
integrates very well with the .NET environment supporting the application stack. Azure 
provides a lot of tools for a variety of requirements (e.g., logging). Whether using
Azure is ideal, however, is a matter of discussion in the group, and is based on both political and personal bias and opinions.

1. evolution and refactoring


2. operation


3. maintenance


* Link back to commit messages/issues to illustrate these. *

