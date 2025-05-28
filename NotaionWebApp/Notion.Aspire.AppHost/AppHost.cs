var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Notaion>("notaion");

builder.Build().Run();
