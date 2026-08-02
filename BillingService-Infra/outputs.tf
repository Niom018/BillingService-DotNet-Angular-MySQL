output "public_ip" {
  description = "Public IP of the server - this is your live URL (http://<this-ip>)"
  value       = aws_eip.billing_service.public_ip
}

output "ssh_command" {
  description = "Command to SSH into the server"
  value       = "ssh -i ${var.key_pair_name}.pem ubuntu@${aws_eip.billing_service.public_ip}"
}
