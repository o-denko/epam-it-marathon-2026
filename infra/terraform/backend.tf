terraform {
  required_version = ">= 1.10"

  backend "s3" {
    bucket       = "restaurant-terraform-state-591015654179"
    key          = "restaurant/terraform.tfstate"
    region       = "eu-north-1"
    use_lockfile = true
    encrypt      = true
  }
}
