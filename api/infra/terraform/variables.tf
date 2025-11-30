variable "project_name" {
  description = "Name prefix for AWS resources"
  type        = string
  default     = "cashify"
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "aws_profile" {
  description = "AWS CLI profile to use"
  type        = string
  default     = "cashify-aws"
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t2.micro"
}

variable "ssh_port" {
  description = "Custom SSH port used in addition to 22"
  type        = number
  default     = 50022
}

variable "ssh_allowed_cidr" {
  description = "CIDR block allowed to SSH"
  type        = string
  default     = "0.0.0.0/0"
}

variable "public_key_path" {
  description = "Path to your local SSH public key"
  type        = string
}

variable "ami_id" {
  description = "Ubuntu 24.04 LTS AMI ID in the chosen region"
  type        = string
  # You already set this in terraform.tfvars:
  # ami_id = "ami-00d8fc944fb171e29"
}
