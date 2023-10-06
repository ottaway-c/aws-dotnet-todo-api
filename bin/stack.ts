#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import * as dotenv from "dotenv";
import { AppStack } from "../lib/app-stack";
import { EcrStack } from "../lib/ecr-stack";
import { get } from "env-var";

dotenv.config();

const CDK_DEFAULT_ACCOUNT = get("CDK_DEFAULT_ACCOUNT").required().asString();
const CDK_DEFAULT_REGION = get("CDK_DEFAULT_REGION").required().asString();
const SERVICE = get("SERVICE").required().asString();
const STAGE = get("STAGE").required().asString();
const TAG = get("TAG").required().asString();

const ecrStackName = `${SERVICE}-${STAGE}-ecr`;
const appStackName = `${SERVICE}-${STAGE}-app`;

const app = new cdk.App();

const ecrStack = new EcrStack(app, ecrStackName, {
  description: `${SERVICE} ${STAGE} ecr stack`,
  service: SERVICE,
  stage: STAGE,
  env: {
    account: CDK_DEFAULT_ACCOUNT,
    region: CDK_DEFAULT_REGION,
  },
});

new AppStack(app, appStackName, {
  description: `${SERVICE} ${STAGE} application stack`,
  service: SERVICE,
  stage: STAGE,
  functionRepository: ecrStack.functionRepository,
  tag: TAG,
  env: {
    account: CDK_DEFAULT_ACCOUNT,
    region: CDK_DEFAULT_REGION,
  },
});
