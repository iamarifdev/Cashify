output "instance_id" {
  description = "EC2 instance ID"
  value       = aws_instance.this.id
}

output "public_ip" {
  description = "Public IP of the EC2 instance"
  value       = aws_instance.this.public_ip
}

output "key_name" {
  description = "SSH key pair name"
  value       = aws_key_pair.this.key_name
}

output "ssh_command" {
  description = "SSH command to connect to the instance"
  value       = "ssh -i ${var.public_key_path} ubuntu@${aws_instance.this.public_ip}"
}

output "connect_info" {
  description = "Connection information"
  value       = "Key: ${aws_key_pair.this.key_name}, IP: ${aws_instance.this.public_ip}, User: ubuntu"
}
