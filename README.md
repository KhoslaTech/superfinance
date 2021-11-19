Introduction
--------------------

This is a sample web application built on [ASP.NET Core MVC](https://github.com/dotnet/aspnetcore) using [ASPSecurityKit](https://ASPSecurityKit.net/) (a cross-platform IAM framework to rapidly build reliable web apps and API platforms with enterprise-grade security).

SuperFinance is a full-fledged SaaS web platform prototype that provides self-service digital banking services portal for banking institutions and customers to setup and manage banks, staff, accounts, nominees (beneficiaries) and perform transactions. It incorporates advance security controls such as [two-factor authentication](https://ASPSecurityKit.net/features/#mfa), [rules-based account suspension](https://ASPSecurityKit.net/features/#entity-suspension), [IP firewall](https://ASPSecurityKit.net/features/#ip-firewall), [email verification](https://ASPSecurityKit.net/features/#email-verification), [XSS](https://ASPSecurityKit.net/features/#xss) and so on.

It employs ASPSecurityKit's revolutionary [Activity-Data Authorization (ADA)](https://ASPSecurityKit.net/features/#ada) component for authorization to ensure that each user within the system can only perform operations and access data that belong to her.

The sample also leverages ASPSecurityKit [Premium source package](https://ASPSecurityKit.net/docs/source-packages/?packageId=premium-netCoreMvc#available-packages) to jump straight into building the banking aspect of the platform and takes advantage of the [several key security workflows](https://ASPSecurityKit.net/docs/source-packages/?packageId=premium-netCoreMvc#operations) that comes with the Premium package as source code.

Live demo
--------------------

Visit [https://SuperFinance.ASPSecurityKit.net](https://SuperFinance.ASPSecurityKit.net/) to play with a live demo based on this sample.

Step-by-step tutorial
--------------------

A [step-by-step tutorial](https://ASPSecurityKit.net/samples/superfinance/) is also available, which takes you through 'building SuperFinance from scratch' and in the process, you learn important security related concepts to help you master ASPSecurityKit to rapidly build reliable and secure web applications.

Running the sample
--------------------

### Prerequisits
* [Visual Studio](https://visualstudio.microsoft.com/) 2019 or higher
* [SQL Server Express](https://www.microsoft.com/en-in/sql-server/sql-server-downloads) (built with v14, but latest should work)

### Steps:
* First, clone this repo or download it as zip file.
* Open the SuperFinance.sln in Visual Studio 2019 or higher.
* Make sure that the [Node and Grunt dependencies are installed](https://aspsecuritykit.net/docs/using-the-aspsecuritykit.tools/#install-node-dependencies) to build scripts and css resources.
* From solution explorer, Open appsettings.json and,
    - Make sure that the connectionString for the SuperFinance database is valid for your machine.
    - For email related features to work, you need to set credentials in `MailId` and `MailPassword` of some Gmail account.
* In Package Manager Console, execute the following command to create the database:
```ps1
update-database -context DemoDbContext
```
* Press F5 to run in debug mode.
    - This sample can only be run on localhost:&lt;port&gt; host-based URL as per the [trial license](#user-content-trial-license).

Feedback
--------------------

Feedback is much appreciated. For any question, to report any issue or need some help, feel free to get in touch on [support@ASPSecurityKit.net](mailto:support@ASPSecurityKit.net)

License
--------------------

This sample source code is licensed under [KHOSLA TECH - END USER AGREEMENT](https://aspsecuritykit.net/legal/end-user-agreement/)

You're free to learn and incorporate techniques and logic available in this sample in projects that uses a [licensed](https://aspsecuritykit.net/docs/license/#license-key) version of ASPSecurityKit.

Since ASPSecurityKit's [source packages](https://aspsecuritykit.net/docs/source-packages/#compare-packages) are also paid products, we've moved a good portion of the source code that comes with the [Premium package](https://ASPSecurityKit.net/docs/source-packages/?packageId=premium-netCoreMvc#available-packages) into a demo library for this sample. This demo library can only be used to run and evaluate SuperFinance sample.

<a name="trial-license"></a>The [trial license key](https://ASPSecurityKit.net/docs/license/#trial-key) in the sample lets you run the sample in Visual Studio. It's only meant for this SuperFinance sample and cannot be used for any other project.
