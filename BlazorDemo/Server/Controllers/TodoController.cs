﻿using BlazorDemo.Data.Models.Tables;
using BlazorDemo.Server.Services.Interfaces;
using BlazorDemo.Shared.Models.Todo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorDemo.Server.Controllers;

[ApiController, Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger)
	{
        _todoService = todoService;
        _logger = logger;
    }

    [HttpGet("[action]")]
    public IAsyncEnumerable<TodoItemViewModel> GetAll()
        => _todoService.QueryAll(TodoItemViewModel.Selector).AsAsyncEnumerable();

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateOrUpdate([FromBody] TodoItemViewModel viewModel)
    {
        if (!ModelState.IsValid) {
            return Problem("Todo item is invalid.");
        }

        try {
            TodoItem item = viewModel.ToEntity();
            await _todoService.UpsertAsync(item);

            return Ok();
        }
        catch (Exception e) {
            _logger.LogError(e, "Error saving Todo item.");
            return Problem("Todo item could not be saved due to an unexpected error.");
        }
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Complete(int todoItemId)
    {
        try {
            await _todoService.CompleteAsync(todoItemId);
            return Ok();
        }
        catch (Exception e) {
            _logger.LogError(e, "Error completing Todo item.");
            return Problem("Todo item could not be completed due to an unexpected error.");
        }
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> Delete(int todoItemId)
    {
        try {
            await _todoService.DeleteAsync(todoItemId);
            return Ok();
        }
        catch (Exception e) {
            _logger.LogError(e, "Error deleting Todo item.");
            return Problem("Todo item could not be deleted due to an unexpected error.");
        }
    }
}
