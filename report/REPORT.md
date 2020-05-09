# Development Report

This document is the final report of Group b (2k20 MSc) for the _DevOps,
Software Evolution and Software Maintenance_ course at IT University of
Copenhagen, held in spring 2020. It will detail the process of migrating the
Minitwit platform (a really simple social network similar to Twitter),
refactoring of its system, major devops tasks we have done throughout the
course, our team and repository organization and strategies. Report is divided
by sections, not by the timeline, so the reader can get better overview of our
work. We also argued for the choice of technologies and decisions we had made.
At the end of the document, we give a summary of what were our biggest issues
and challenges, what we have learned out of that and what could have done
better.

## Technology Stack

_TODO - dependencies and important interactions of subsystems_

### Hosting

As the application should be publically available IP address we needed a hosting
provider for the production environment. The course presented Digital Ocean as
an option, and as a couple of us had prior experience with them we choose this
solution. The prior experience along with the option to utilize the Github
Education Pack for free credits meant we had confidence in the decision.

We started out provisioning a small droplet, which is what Digital Ocean names
their virtual private servers (VPS), with enough resources to host the
application and the database inside Docker containers. However, whenwe added
monitoring and logging to application we had increasing requirements for the
specs of the VPS. To meet these requirements we provisioned multiple droplets as
scaling a single VPS vertically would introduce downtime, which we wanted to
avoid.

The result of this approach led us to having two droplets; one for our tools
(e.g., logging) and one for the solution. In retrospect this was probably the
right decision as it increases our resiliency. If one of the droplets crashes
the other one remains untouched.

Despite the short-term benefit of this solution we still have issues scaling the
droplet hosting the application. Because the application isn't stateless
horizontal scaling isn't an option, and thus vertical scaling is our sole
option, which requires us to incur some downtime when the droplet is upgraded.

### Operating system

For the operating system of the application droplet we decided on Ubuntu 18.04.3
LTS. It was important for us to use a version with long-term support (LTS) as it
helps us ensure stability and reliability as well as active support should bugs
or security vulnerabilties surfaced during the course.

We wanted to use a Linux based distribution as it seemed to provide the greatest
level of learning. Using Windows based virtual machine could provide us a
graphical user interface and tools, and it would definitely yield some valuable
learning as well it seemed a less attractive choice in the context of this
course and tools we aimned to utilize. The choice of Ubuntu is definitive as we
could have used Arch Linux or any other distribution. However, Ubuntu is rather
common and has a great community making tools, which make it easier to figure
out how tasks are done. These considerations combined made Ubuntu an ideal
choice for the members of the group that didn't have a extensive experience with
the OS and thus a more limited level of proficiency with it.

Due to the technologies we were planning on using (i.e., Docker) we weren't
going to be working too much directly on the operating system level. This meant
that we didn't require to have one of the group members focusing on the OS more
than others. It also proved to be a comfortable environment for the group
members used to working in Windows.

### Containerization

To run the application and the database instance required by the application we
decided on the use of Docker. This choice rested primarily on it being presented
in the course, but a more important fact was that all group members had interest
in using the technology, and a good introduction to the fundamentals around
containers, which has broad applicability in other technologies such as
Kubernetes.

In a professional context it might have been a better choice to use some
containerization-as-a-service solution or a provider where the features
underlying operating system support Docker is managed. Sticking with installing,
configurating, and operating Docker was a lot more involved than a managed
solution in terms of manual work, but it provided us with invaluable learning
opportunities.

There are other alternatives to Docker, but it is the primary technology
supporting containerizatio and thus an unoffical standard in the business. An
example of an alternative is Vagrant used to provision the servers, but we
deemed it less attractive as it is a rather heavy-weight solution (i.e., entire
operating system) in order to gain the same isolation Docker provides.

**Docker Swarm** We choose Docker Swarm as the technology used to scale the
system. As this step was required later in the course the choice integrates
beautifully with our prior investment into Docker Compose. To keep the setup
simple we decided to run Docker Swarm with a single node (the original host
machine) to act as the swarm manager and only worker as it also hosts the web
applicaton and database.

We didn't invest too much time looking into alternatives as Docker Swarm seemed
to provide all the tools necessary with less technical fragmentation (i.e.,
using several different providers with differing configuration systems).

**Evaluation** In accordance with your prior interest in Docker the choice of
Docker Swarm was a natural extension of this. The main hurdles encountered is
the isolated knowledge of the technology. Due to nature of it we allocated a
single person to set it up, which meant that when an error was encountered by
others in the group we didn't have a clear picture. However, this can be helped
by documenting the approach taken and sharing lessons learned.

When considering the choice of using a single node there are some consequences
we are aware of, but which means true scaling and reliability isn't archieved.
In the case that the node crashes is the entire system also taken down. The
scaling aspect is also constrained to the resources available on that single
node. Both of these issues can be resolved by adding more physical machines to
the swarm, which we can do seamlessly because of docker swarm. However, this was
a deliberate decision in order to keep monetary costs down.

### Programming language & Runtime environment

Before starting the refactoring of the existing MiniTwit application we
considered our options and interests of the group members in relation to the
programming language we aimed to use. This of course had an impact on our
choices of web application frameworks available to us.

We ended up using .NET Core 3.1 with C# as it was argued that it was the
language that most of the group members would be able to write from the start.
As mentioned a couple of times in previous sections we wanted to focus less on
the development of the application and more on setting up the DevOps tools and
processing related to it, so chosing a completely new, and thus challenging
language wasn't a priority.

The choice of the C# naturally led us to the usage of the ASPNET Core web
framework. This framework provides us with good documentation on authoring both
server-rendered pages and REST APIs. For interaction with the database we
decided to use an ORM rather than handwritten SQL statements for reasons
regarding both security and speed of development. The choice of ORM ended on
Entity Framework Core as it integrates very well with ASPNET Core, and has
adapters to many different database giving us freedom in chosing our storage
solution later.

### Database

**Microsoft SQL Server** We did not want to use SQLite in production, so we had
to change to another database scheme. Due to already picking C# for the
programming language, it seemed to make sense to continue the development in the
same technology sphere, by picking yet another microsoft product. This also
provided the best possible object relation mapping (ORM), which made development
somewhat easier.

**Evaluation** The only problem we've had with this, was that a subset of our
development team continued to use SQLite for local testing, and SQLite has a
more relaxed relation to constraint, so some errors would occure in production
that didn't locally. However we fixed this by making it easier to spin up a
MSSQL database locally. However MSSQL in itself provided no problems - it had
exactly the features that we were after, and worked like a charm. This seemed to
have been a good choice.

### Monitoring

**Monitoring: Prometheus & Grafana** We chose Prometheus as the primary
monitoring tool and Grafana for data visualization. No one from the team had a
lot of experience working with monitoring tools. It meant that no one had
expectations or any preference regarding choosing the monitoring tool. Taking a
look at [Prometheus's comparison to
alternatives](https://prometheus.io/docs/introduction/comparison/) (even though
it should be taken with a grain of salt as they made the comparison themself) it
made it clear that it did fit into the setup because Prometheus is designed to
monitor targets as in servers, containers and the like.

Also, supporting active monitoring by periodically scrap our application by
pulling data from this target. A pull-based system enables us to rate control in
which it will pull the data. With a push-based system we may have the risk of
sending too much data towards our server and in worst case crash it.

To set up Prometheus and Grafana is just a matter of creating docker containers.

**Evaluation** In our case, it must integrate well into our current setup, and
it is well-supported. That is, there is some kind of official library for the
tool that we want to use, and it is actively maintained. [Prometheus has an
official GitHub repository for
.NET](https://github.com/prometheus-net/prometheus-net) with examples of getting
started which fits with our criteria. Alternatives like Graphite there is no
official GitHub repository, and a [simple search on
Github](https://github.com/search?q=graphite+.net) reveals it. It also the fact
that if the community is large enough there is a possibility of finding a
solution to your problem in a short amount of time.

### Logging

**ELK** We chose Elasticsearch, Logstash and Kibana (the ELK stack) as logging
tools. Firstly, it enables us to do modern, scalable and user-friendly logging,
and it is also one of the most popular choices at the moment. Because it
promotes centralized logging we chose to set up the ELK stack on another
droplet, and the fact that Elasticsearch requires a larger amount of memory.

To setup the ELK stack is just a matter of creating docker containers.

**Evaluation** As we prioritize ease of integration of the toolset that we
choose, it felt that the ELK stack was the right choice. We tried to look for
alternatives to avoid making the easy choice of choosing the most popular one.
We had a look at [LogDNA](https://logdna.com) that is Elasticsearch and Kibana
combined. It also supports containerized environments even though they suggest
using Kubernetes with their product. We want to ensure that libraries used for
this product are well-supported and maintained for .NET. We found a Git
Repository named [RedBear.LogDNA](https://github.com/RedBearSys/RedBear.LogDNA)
where a library resides for connecting to logDNA from .NET. Studying the issues
that were made did make the choice easy for us as some issues stated that the
library leads to system failure was not something that we wanted to work with.
Furthermore, it should be simple to acquire help which did not look like to be
the case.

## System Description

_TODO - design and architecture of our ITU-MiniTwit system_

### Monitoring

_TODO - What do we monitor in our system_

### Logging

_TODO - what do we log and how we aggregate it_

### Scaling and load balancing

_TODO - Which strategy did we use for scaling and load balancing (ie. vertical
vs. horizontal scaling)_

## CI/CD Pipeline

Having a strong CI/CD pipeline is a crucial development step in an agile
development team. Having automated deployment removes operations time from
having to manual deploy to production, it also limits potential errors.
Combining that with a variety of automated tests, makes sure that our production
environment is as close to perfect as possible. This, however, depends on the
quality of the tests and the various tools utilized.

This section will go through the choices we made relating to the pipeline, and
how it integrated in our workflow.

To understand the pipeline, it is first important to understand the context. The
following diagram illustrates a generic development flow.

![Development Flow](./images/development_diagram.png)

The pipeline is only a small part of the development flow, however, done
correct, it increases the effectiveness ten-fold.

### Continuous Integration

After any commit is pushed to the remote repository, the CI tests check if the
new feature introduces any regression, and if code quality is of satisfactory
levels. This is an important part of our review process of each feature - this
alleviated some pressure from the peer-reviewer, making it easier to trust that
the feature did not break anything - this cut down on time spent on reviews,
leaving room for developing features and operations.

#### Github Actions

Continuous integration required the ability to run a variety of scripts,
essentially, when ever a new commit enters the repository. We initially looked
at a variety of different CI possibilities, and considered Jenkins for one,
however we randomly looked at github and a group member asked if any of us have
every tried using Github Actions, their CI solution, and we realized that
neither of us had. This seemed like a great way to try something new and it also
seemed ideal with the current stack we were running. The choice was mainly based
on the availability and the possibility to learn something new.

We created three Github Action, whereas the following file is related to the
automated tests:

```YAML
name: Automated Tests

on: [push]

jobs:
  build:

    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET Core
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 3.1.100
    - name: Run integration tests
      working-directory: ./src/
      run: docker-compose -f docker-compose.test.yml up --build --exit-code-from tester
    - name: Run unit tests
      run: dotnet test
      working-directory: ./src/WebApplication.Tests
```

#### Unit tests

To help increase our confidence in the changes added to the system we added unit
tests. The unit-tests are written in C# using the XUnit test framework, which is
used to test functionality as implemented in web application (e.g., creating a
user, adding a message, etc.).

The unit tests are focused around the service classes, which implements the
business logic related to the functionality of the system. These tests aims to
check the "happy-path" where the function succeeds as well as the expec- ted
error paths (i.e., adding a message to an unknown user). The tests are executed
using the built-in tooling of the dotnet CLI included in the .NET Core SDK.

These tests are run as part of the CI pipeline on pull requests (PR), and when a
PR is merged into the master branch.

#### Integration tests

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

### Continuous Delivery

When new commits enters the master branch, we needs to update the production
server. The alternative would be to manually connect to the production
environment and pull the newest version of the system. This takes time and is
error prone, therefore this flow is automated. This is, in our case, done by
releasing a version of the code, organized by a docker image, to a package host,
which could then be downloaded by our production server.

**Github, as a packaging host**, has the possibility to host and provide
packages for downloading, much like Dockerhub. We considered moving our images
to Dockerhub, but having packages stored the same place as storing the
repository seemed ideal. We liked the idea of having a few select services that
we relied on, as to not create a too complex development flow, but rather
utilize few tools that integrated nicely together and provided each a large
chunk of the features required.

**Our github action**, automated the creation of the docker image as well as
pushing the image to the packaging host, seen
[here](https://github.com/Trivivium/devops-2k20/blob/master/.github/workflows/dockerpush.yml).
When pushed, the latest release can be accessed
[here](https://github.com/Trivivium/devops-2k20/packages/133732).

We had an [additional
action](https://github.com/Trivivium/devops-2k20/blob/master/.github/workflows/release.yml)
to create release [tags](https://github.com/Trivivium/devops-2k20/releases).

Upon the package being pushed to the packaging host, the next step is for the
production environment to pull the image.

**Watchtower** is a tool that automatically updates the base image of Docker
containers, which can be found [here](https://github.com/containrrr/watchtower).
There are a few different tools for this, and this is something Jenkins does
provide, however installing Jenkins for this feature alone, seemed overkill and
would use a magnitude more CPU power than we wanted. We wanted to utilize a
minimal tool that provides what was needed, and nothing else. This is what
watchtower did. It checks your package host with a consistant interval checks
whether the remote image has been updated - if it has, it updates the running
container. The setup was really simple, here showing the full configuration in
our `docker-compose`
[file](https://github.com/Trivivium/devops-2k20/blob/master/src/production.yml):

```yml
watchtower:
  image: v2tec/watchtower
  volumes:
    - /var/run/docker.sock:/var/run/docker.sock
    - /root/.docker/config.json:/config.json
  command: --interval 300
```

Worth noting, however, is that Watchtower does this by connecting to the docker
socket, making it able to spy on and interact with all running containers. This
is naturally dangerous if there is a security bug in watchtower, or if it is in
any way acts maliciously. We, however, trusted the service in the context of
this course, but that is a security estimate that should be considered in
projects.

The push step is easily visualized on the repository, making it transparent
whether the package creation was successful, however we have no way to monitor
whether the production is updated, nor the version of the software is running,
other than SSH'ing into the production server and checking.

## Development practices
With the Covid-19 epidemic emerging approximately half-way through the course,
the interaction amongst us inevitably changed as it was unwise to meet
physically. Initially when the course started, we would hold a meeting after the
lecture to plan the steps which needed to be accomplished for the following
weeks release.

As Covid-19 progressed, ITU closed down, and gatherings became rightfully
frowned upon which forced us to settle for the suboptimal approach of talking
over Zoom. While the content of these meetings were almost identical to that of
our physical meetings, the overhead of technical issues involving subpar
microphones and a internet connections made these meetings far less efficient.
These meetings over Zoom took place approximately once every week. Meetings over
Zoom weren’t always necessary, which is why we also used Slack to keep each
other updated on progress on issues as well as to ask for consent to make
changes to the system or to ask for help.

The typical output of the meetings was an understanding of what tasks needed to
be carried out and by whom. These tasks would be posted as issues on Github,
where a group member would assign themselves to it. Using Github issues rather
than other tools, was an easy choice for us as it provided the functionality we
needed while also minimizing the spread of tools we used, seeing as we also used
Github for version control.

The vast majority of the issues were handled independently without pair
programming, which is definitely something we should have done in hindsight, as
mistakes could have been avoided and knowledge of more intricate details in our
program could be shared more conveniently. Although we didn’t define any clear
roles for the group members, the lack of pair programming resulted in some
intrinsic roles when the issues were perhaps larger than they should have been.
Being more consistent with creating smaller issues, could have solved this
problem, as multiple people would then have the opportunity of working on the
same subsystem.

When working on an issue that is some new feature in the project, we created a
new branch to work on it in a separate environment. Once the issue was deemed
complete, a pull request was made to finally merge the new feature into the
master branch. This branching strategy is very much in line with the ‘Topic
Branches’ model where branches are short-lived and become merged with the master
branch once the feature is working as intended. 
 
**Issue tracking / Kanban: Github** Generally speaking we never really put much
thought into how we would track issues and how we would separate the tasks at
hand. As we already had Github open, we simply created all our tasks on the
issue board there and never thought about alternatives. Alternatively we could
have created a Trello board or a Jira project, however with the limited scope of
the project it seemed extensive to include a whole other system just for task
management. As previously mentioned we generally tried limiting the number of
different tools we used, and create a stack with as few different tools as
possible.

**Evaluation** We definitely had problems with our taskmanagement and ended up
doing some of the tasks too late, so we definitely had to change our workflow,
and would have if we could do it over. I think the main issue was that we didn't
consult the issue list often enough, and possibly didn't put deadlines on, as
well as not assigning people to issues. Ideally we should probably have improved
our overall development process earlier on, but this is covered in the [Post
Mortem](../postmortem.md). We probably wouldn't have gotten any alternative
important features by choosing another service, as the problems we had were
based on structural team problems rather than the tool itself. Having the issues
closely aligned with the pull-request flow was definitely a helpful feature.

## State of solution

_TODO - current state of our system, results of static analysis and code quality
assessment, add security assessment too_

#### CI/CD

