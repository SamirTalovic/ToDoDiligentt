import { create } from "zustand";
import { CreateTodoItem, TodoItem } from "../common/interfaces/TodoItemInterface";
import agent from "../api/agent";
import * as signalR from "@microsoft/signalr";

interface TodoItemStoreState {
  todoItems: TodoItem[];
  selectedTodo: TodoItem | null;
  loading: boolean;
  loadingInitial: boolean;
  hubConnection: signalR.HubConnection | null;
  initializeSignalRConnection: () => void;
  loadTodoItems: () => Promise<void>;
  loadTodoItem: (id: number) => Promise<void>;
  createTodoItem: (todoItem: CreateTodoItem) => Promise<void>;
  updateTodoItem: (todoItem: TodoItem) => Promise<void>;
  deleteTodoItem: (id: number) => Promise<void>;
}

export const useTodoItemStore = create<TodoItemStoreState>((set, get) => ({
  todoItems: [],
  selectedTodo: null,
  loading: false,
  loadingInitial: false,
  hubConnection: null,

  initializeSignalRConnection: () => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_APP_API_URL + "/todoHub")
      .withAutomaticReconnect()
      .build();

    hubConnection.on("ReceiveTodoUpdate", (id: number, title: string, description: string, createdAt: Date, appUserId: string) => {
      set(state => {
        const updatedItem = state.todoItems.find(item => item.id === id);
        if (updatedItem) {
          return {
            todoItems: state.todoItems.map(item =>
              item.id === id ? { ...item, title, description, createdAt: new Date(createdAt), appUserId } : item
            )
          };
        } else {
          return {
            todoItems: [...state.todoItems, { id, title, description, createdAt: new Date(createdAt), appUserId }]
          };
        }
      });
    });

    hubConnection
      .start()
      .then(() => set({ hubConnection }))
      .catch(err => console.error("SignalR Connection Error: ", err));
  },

  loadTodoItems: async () => {
    set({ loadingInitial: true });
    try {
      const todoItems = await agent.TodoItemsRequests.getAll();
      set({ todoItems, loadingInitial: false });
    } catch (error) {
      console.error(error);
      set({ loadingInitial: false });
    }
  },

  loadTodoItem: async (id: number) => {
    set({ loading: true });
    try {
      const todoItem = await agent.TodoItemsRequests.getById(id);
      set({ selectedTodo: todoItem, loading: false });
    } catch (error) {
      console.error(error);
      set({ loading: false });
    }
  },

  createTodoItem: async (todoItem: CreateTodoItem) => {
    set({ loading: true });
    try {
      const createdItem = await agent.TodoItemsRequests.create(todoItem);
      set(state => ({
        todoItems: [...state.todoItems, createdItem],
        loading: false
      }));
    } catch (error) {
      console.error(error);
      set({ loading: false });
    }
  },

  updateTodoItem: async (todoItem: TodoItem) => {
    set({ loading: true });
    try {
      await agent.TodoItemsRequests.update(todoItem.id, todoItem);
      set(state => ({
        todoItems: state.todoItems.map(item => (item.id === todoItem.id ? todoItem : item)),
        selectedTodo: todoItem,
        loading: false
      }));
    } catch (error) {
      console.error(error);
      set({ loading: false });
    }
  },

  deleteTodoItem: async (id: number) => {
    set({ loading: true });
    try {
      await agent.TodoItemsRequests.delete(id);
      set(state => ({
        todoItems: state.todoItems.filter(item => item.id !== id),
        loading: false
      }));
    } catch (error) {
      console.error(error);
      set({ loading: false });
    }
  }
}));
