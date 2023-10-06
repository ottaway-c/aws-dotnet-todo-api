import * as cdk from "aws-cdk-lib";
import * as ecr from "aws-cdk-lib/aws-ecr";
import { Construct } from "constructs";

export interface EcrStackProps extends cdk.StackProps {
  stage: string;
  service: string;
}

export class EcrStack extends cdk.Stack {
  public functionRepository: ecr.Repository;

  constructor(scope: Construct, id: string, props: EcrStackProps) {
    super(scope, id, props);

    this.functionRepository = new ecr.Repository(this, "FunctionRepository", {
      repositoryName: `${this.stackName}-function-repository`,
      encryption: ecr.RepositoryEncryption.AES_256,
      imageScanOnPush: true,
      lifecycleRules: [
        {
          rulePriority: 1,
          description: "Remove if more than 3 images",
          tagStatus: ecr.TagStatus.ANY,
          maxImageCount: 3,
        },
      ],
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteImages: true,
      imageTagMutability: ecr.TagMutability.MUTABLE,
    });
  }
}
