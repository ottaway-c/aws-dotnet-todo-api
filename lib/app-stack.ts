import * as actions from "aws-cdk-lib/aws-cloudwatch-actions";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as cdk from "aws-cdk-lib";
import * as cloudwatch from "aws-cdk-lib/aws-cloudwatch";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import * as ecr from "aws-cdk-lib/aws-ecr";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as logs from "aws-cdk-lib/aws-logs";
import * as sns from "aws-cdk-lib/aws-sns";
import { Construct } from "constructs";

export interface AppStackProps extends cdk.StackProps {
  stage: string;
  service: string;
  functionRepository: ecr.Repository;
  tag: string;
}

export class AppStack extends cdk.Stack {
  private props: AppStackProps;

  constructor(scope: Construct, id: string, props: AppStackProps) {
    super(scope, id, props);
    this.props = props;

    const snsTopic = new sns.Topic(this, "SnsTopic", {
      topicName: `${this.stackName}-alarm`,
    });
    snsTopic.applyRemovalPolicy(cdk.RemovalPolicy.DESTROY);

    const dynamoTable = new dynamodb.Table(this, "DynamoTable", {
      tableName: `${this.stackName}-todo-table`,
      billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
      encryption: dynamodb.TableEncryption.AWS_MANAGED,
      pointInTimeRecovery: true,
      partitionKey: {
        name: "PK",
        type: dynamodb.AttributeType.STRING,
      },
      sortKey: {
        name: "SK",
        type: dynamodb.AttributeType.STRING,
      },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    dynamoTable.addGlobalSecondaryIndex({
      indexName: "GSI1",
      partitionKey: {
        name: "GSI1PK",
        type: dynamodb.AttributeType.STRING,
      },
      sortKey: {
        name: "GSI1SK",
        type: dynamodb.AttributeType.STRING,
      },
      projectionType: dynamodb.ProjectionType.ALL,
    });

    const lambdaFunction = new lambda.DockerImageFunction(this, "LambdaFunction", {
      code: lambda.DockerImageCode.fromEcr(props.functionRepository, {
        tagOrDigest: props.tag,
      }),
      functionName: `${this.stackName}-api`,
      architecture: lambda.Architecture.X86_64,
      timeout: cdk.Duration.seconds(30),
      memorySize: 1024,
      environment: {
        SERVICE: props.service,
        STAGE: props.stage,
      },
      tracing: lambda.Tracing.ACTIVE,
    });

    new logs.LogGroup(this, "LambdaFunctionLogGroup", {
      logGroupName: `/aws/lambda/${lambdaFunction.functionName}`,
      retention: props.stage === "prod" ? logs.RetentionDays.ONE_YEAR : logs.RetentionDays.ONE_WEEK,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    const lambdaFunctionAlias = lambdaFunction.addAlias("LIVE");

    dynamoTable.grantReadWriteData(lambdaFunctionAlias);

    const api = new apigateway.RestApi(this, "ApiGateway", {
      restApiName: this.stackName,
      deployOptions: {
        stageName: "LIVE",
        tracingEnabled: true,
      },
      defaultCorsPreflightOptions: {
        allowMethods: apigateway.Cors.ALL_METHODS,
        allowOrigins: apigateway.Cors.ALL_ORIGINS,
        allowHeaders: apigateway.Cors.DEFAULT_HEADERS,
        maxAge: cdk.Duration.seconds(60),
      },
      cloudWatchRole: false,
    });
    api.applyRemovalPolicy(cdk.RemovalPolicy.DESTROY);

    api.root.addProxy({
      defaultIntegration: new apigateway.LambdaIntegration(lambdaFunctionAlias),
      anyMethod: true,
    });

    const apiErrors = new cloudwatch.Alarm(this, "ApiErrors", {
      alarmDescription: "500 errors > 0",
      metric: api.metricServerError({ period: cdk.Duration.minutes(1) }),
      threshold: 1,
      evaluationPeriods: 1,
      actionsEnabled: true,
      comparisonOperator: cloudwatch.ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
    });
    apiErrors.applyRemovalPolicy(cdk.RemovalPolicy.DESTROY);

    apiErrors.addAlarmAction(new actions.SnsAction(snsTopic));
  }
}
