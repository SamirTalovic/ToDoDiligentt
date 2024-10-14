export interface TodoItem {
    id: number;
    title: string;
    description: string;
    createdAt: Date;
    appUserId: string; 
}

export interface CreateTodoItem {
    title: string;
    description: string;
    createdAt: Date;
    appUserId: string; 
}
