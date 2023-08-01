using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if(request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must suppply a valid object"));

        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(toDoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if(request.Id <=0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));
        
        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(toDoItem != null)
        {
            return await Task.FromResult(new ReadToDoResponse{
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var toDoItems = await _dbContext.ToDoItems.ToListAsync();

        foreach(var toDo in toDoItems)
        {
            response.ToDo.Add(new ReadToDoResponse{
                Id = toDo.Id,
                Title = toDo.Title,
                Description = toDo.Description,
                ToDoStatus = toDo.ToDoStatus
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty )
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must suppply a valid object"));
        
        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));

        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.ToDoStatus = request.ToDoStatus;

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse{
            Id = toDoItem.Id
        });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if(request.Id <=0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));
        
        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));

        _dbContext.Remove(toDoItem);

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse{
            Id = toDoItem.Id
        });

    }
}