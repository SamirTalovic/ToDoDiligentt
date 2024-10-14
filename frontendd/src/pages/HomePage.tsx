import React, { useState, useEffect } from 'react';
import { observer } from 'mobx-react-lite';
import { CreateTodoItem, TodoItem } from '../common/interfaces/TodoItemInterface';
import { store } from '../stores/store';
import TodoItemStore from '../stores/todoStore';
import * as signalR from "@microsoft/signalr";

const HomePage = observer(() => {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [loading, setLoading] = useState(false);

    const todoItemStore = store.todoStore as TodoItemStore;

    const fetchTodoItems = async () => {
        setLoading(true);
        try {
            await todoItemStore.loadTodoItems();
        } catch (error) {
            console.error('Failed to fetch todo items:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchTodoItems();   

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(`${import.meta.env.VITE_APP_API_URL}/todoHub`, {
                withCredentials: true
            })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveTodoUpdate", (id: number, title: string, description: string, createdAt: string) => {
            const createdAtDate = new Date(createdAt);
            todoItemStore.updateTodoItem({ id, title, description, createdAt: createdAtDate } as TodoItem);
        });

        connection.start().catch(err => console.error("SignalR Connection Error: ", err));

        return () => {
            connection.stop().catch(err => console.error("SignalR Disconnection Error: ", err));
        };
    }, [todoItemStore]);

    const handleCreateTodo = async () => {
        if (!title || !description) {
            alert('Title and description are required.');
            return;
        }

        try {
            const newTodoItem: CreateTodoItem = {
                title,
                description,
                createdAt: new Date(),
                appUserId: store.userStore.user?.id || '',
            };

            await todoItemStore.createTodoItem(newTodoItem);
            alert('Todo created successfully!');
            setTitle('');
            setDescription('');
        } catch (error) {
            console.error('Failed to create todo item:', error);
        }
    };

    return (
        <div>
            <h1>Todo List</h1>
            <div>
                <h2>Create Todo Item</h2>
                <input
                    type="text"
                    placeholder="Title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                />
                <textarea
                    placeholder="Description"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                />
                <button onClick={handleCreateTodo}>Create Todo</button>
            </div>
            <div>
                <h2>Todo Items</h2>
                {loading ? (
                    <p>Loading...</p>
                ) : (
                    <ul>
                        {todoItemStore.todoItems.map((item: TodoItem) => (
                            <li key={item.id}>
                                <h3>{item.title}</h3>
                                <p>{item.description}</p>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </div>
    );
});

export default HomePage;
