# aws-dotnet-todo-api

## Overview

A simple Todo REST API that leverages API Gateway, Lambda and DynamoDB.

### Design considerations:

- The entire application is deployed as a container running in AWS Lambda.
- I opted to use the request, endpoint, response ([REPR](https://deviq.com/design-patterns/repr-design-pattern)) design pattern.
- Input must be validated validated using fluent validation.
- Endpoints should be easy to test.
- Structured logging using the Serilog logging library.

### I have used the latest .NET language/compiler features such as:

- Nullable value types and use of the 'required' modifier for class properties.
- Compile time checking of potential null reference exceptions.
- Source generators for JSON and object mapping.

### Infrastructure components

- AWS Lambda ⚡
- Elastic Container Repository (ECR)
- API Gateway
- DynamoDb

### AWS CDK

I utilised AWS CDK to provision the required API Gateway, DynamoDB table, ECR repository and Lambda function.

AWS CDK was chosen for the following reasons:

- Type safety and code completion in your IDE of choice.
- Sensible defaults when provisioning resources.
- Easy to integrate with CI/CD such as GitHub Actions.
- Simplifies the granting of IAM permissions and enforces best practices.
- Ability to create more advanced infrastructure without having to write raw CloudFormation.

### Testing Strategy

I've followed a pretty standard approach for testing serverless applications:

- Unit tests — Anything that can be run in memory for example validators
- Integration tests — Test that the code functions correctly against real AWS infrastructure like DynamoDB.
- End to end tests — Test the full application usually against the actual HTTP API. This is important to make sure that IAM permissions have been setup correctly.

For more information about serverless testing I recommend [this blog post](https://theburningmonk.com/2022/05/my-testing-strategy-for-serverless-applications/).

## Github Actions

I have included the following Github Actions that run when creating PR’s into various branches:

- check.yml — Runs on pull requests to the dev branch. This is used to test feature branches. An ephemeral stack is stood up, and integration/e2e tests run against it.
- cleanup.yml — Runs when pull request to the dev branch are closed. Runs 'cdk destroy' to cleanup feature branch stacks.
- dev.yml — Runs when a pull request is merged to the dev branch. Deploys the stack to dev, and runs e2e tests.
- prod.yml — Runs when a pull request is merged to the main branch. Deploys the stack to prod, and runs e2e tests.

## Prereqs

- Install VS Code for working in CDK in Typescript
- Install .Net 8 SDK https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Install Node.js 20 LTS https://nodejs.org/en/
- Install CDK cli tool globally https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html
- Install AWS cli tool https://aws.amazon.com/cli/ and setup default credentials using `aws configure`.

## Install

Install CDK cli

```
npm install -g aws-cdk
```

Install cdk packages

```
npm i
```

Create a `.env` file and add the following variables:

```
AWS_PROFILE=todo-api-docker # This is the profile ECS local endpoints will look for when vending credentials to the docker container. Note that integration/e2e tests will also use these creds when run locally.
AWS_REGION=<AWS_REGION>
CDK_DEFAULT_ACCOUNT=<AWS_ACCOUNT_ID>
CDK_DEFAULT_REGION=<AWS_REGION>
SERVICE=todo-api
STAGE=1001 # This is your ticket/feature branch name like 1001. I also use stages for dev, uat and prod.
TAG=0.0.1
```

Create an instance profile called `todo-api`. Ensure that the account and region match the account and region declared in the envionment variables above.

```
aws configure sso --profile todo-api
```

## Running locally

I have set this up to run with Docker-Compose in Rider. It probably works in Visual Studio as well, but I haven't tried it.

## Synth

Synth is useful during development to test out changes you're making to CDK. It doesn't actually deploy anything, it just
prints out the CloudFormation stack as YAML.

```
aws sso login --profile todo-api
cdk synth todo-api-feat-1008-app --profile todo-api
```

## Deploy

Step 1: Refresh your AWS credentials

```
npm run sso
```

Step 2: Deploy ECR stack

```
cdk deploy todo-api-feat-1008-ecr --profile todo-api
```

Step 3: Authenticate to ECR

```
aws ecr get-login-password --profile todo-api | docker login --username AWS --password-stdin <AWS_ACCOUNT_ID>.dkr.ecr.<AWS_REGION>.amazonaws.com
```

Step 4: Build and push docker image

```
npm run build
npm run push
```

**Note:**

If you make changes to the code locally and wish to deploy the changes to an existing stack, bump the version number assigned to the `TAG` environment variable. E.g

```
TAG=0.0.2 # Increase this version number by 1
```

Step 5: Deploy

```
cdk deploy todo-api-feat-1008-app --profile todo-api
```

## Test

```
npm run sso
npm run unit
npm run integration
npm run e2e
```

## Destroy

**Note:**

All stack resources have been created with a `RemovalPolicy.DESTROY`. This is to ensure a clean teardown when the stack is deleted. In a production scenario, non-ephemeral resources such as DynamoDb tables should be set to `RemovalPolicy.SNAPSHOT` or `RemovalPolicy.RETAIN`.

```
npm run sso
cdk destroy todo-api-feat-1008-app --profile todo-api
cdk destroy todo-api-feat-1008-ecr --profile todo-api
```

## Dotnet Tools

I am are using CSharpier to format C# code

Install CShariper and Kiota

```
dotnet tool restore
```

Upgrading Kiota

```
dotnet tool update microsoft.openapi.kiota
```

Upgrading CSharpier

```
dotnet tool update csharpier
```

Run formatter

```

npm run format
```

## API Client Generation

I am generating the C# API client using code generation.

Run the following command to build a client:

```
npm run gen-client
```
