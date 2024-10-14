using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Playwright;

namespace Tests
{
    public class TodoAppTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        }

        [Fact]
        public async Task ShouldCreateTodoItemSuccessfully()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5000/todos");

            await page.ClickAsync("button:text('Add New Todo')");

            await page.FillAsync("input[name='Title']", "Test Todo");
            await page.FillAsync("input[name='Description']", "Test Description");

            await page.ClickAsync("button:text('Save')");

            var todoText = await page.InnerTextAsync(".todo-item:last-child");
            Assert.Contains("Test Todo", todoText);
        }

        public async Task DisposeAsync()
        {
            await _browser.DisposeAsync();
            _playwright?.Dispose();
        }
        [Fact]
        public async Task ShouldUpdateTodoItemSuccessfully()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5000/todos");

            await page.ClickAsync(".todo-item:first-child button:text('Edit')");

            await page.FillAsync("input[name='Title']", "Updated Todo Title");
            await page.FillAsync("input[name='Description']", "Updated Description");

            await page.ClickAsync("button:text('Save')");

            var todoText = await page.InnerTextAsync(".todo-item:first-child");
            Assert.Contains("Updated Todo Title", todoText);
        }
        [Fact]
        public async Task ShouldDeleteTodoItemSuccessfully()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5000/todos");

            var todoCountBefore = await page.Locator(".todo-item").CountAsync();

            await page.ClickAsync(".todo-item:first-child button:text('Delete')");

            await page.ClickAsync("button:text('Yes')");

            var todoCountAfter = await page.Locator(".todo-item").CountAsync();

            Assert.Equal(todoCountBefore - 1, todoCountAfter);
        }


    }
}
