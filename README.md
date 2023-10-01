
# Agit Task Manager

Agit prerequisite interview projects


## Features

- (requirement) User Register and Login
- (requirement) CRUD Tasks
- EF code first migration
- JWT authorization
- Entity Changelog
- Zero-allocation hashing algorithm (WyHash64)
- Paging with meta-data


## Environment Variables

To run this project, you will need to adjust the following environment variables (appsetting.json) according to your local

`ConnectionStrings:DefaultConnection`


## Run Locally

Clone the project

```bash
  git clone https://github.com/grammade/Agit-TasksService.git
```


Required Depedencies

- .NET 7 Runtime
- MS SQL

Run migration
```bash
  //navigate to project dir
  dotnet ef database update
```

Start the server
```bash
  dotnet build
  dotnet run
```

