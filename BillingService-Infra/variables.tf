variable "aws_region" {
  description = "AWS region to deploy into"
  type        = string
  default     = "eu-north-1" # same region as your earlier Terraform project
}

variable "instance_type" {
  description = "EC2 instance type - t3.micro is free-tier eligible"
  type        = string
  default     = "t3.micro"
}

variable "key_pair_name" {
  description = "Name of an existing EC2 key pair in this region (create one first - see README)"
  type        = string
}

variable "mysql_root_password" {
  description = "Password to set for the MySQL root user on the server"
  type        = string
  sensitive   = true
}

variable "allowed_ssh_cidr" {
  description = "CIDR allowed to SSH in. Restrict this to your own IP once you know it (e.g. 1.2.3.4/32)."
  type        = string
  default     = "0.0.0.0/0"
}
