import axios, { AxiosError, AxiosResponse } from "axios";
import { AuthUserDto, LoginRequestDto, LoginResponseDto, RegisterRequestDto } from "../common/interfaces/AuthInterface";
import { toast } from "react-toastify";
import { store } from "../stores/store";
import { CreateTodoItem, TodoItem } from "../common/interfaces/TodoItemInterface";

axios.defaults.baseURL = import.meta.env.VITE_APP_API_URL;

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

axios.interceptors.request.use(config => {
    const token = store.commonStore.token;
    if (token && config.headers) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

axios.interceptors.response.use(async response => {
    return response;
}, (error: AxiosError) => {
    const { data, status, headers } = error.response as AxiosResponse;

    if (status) {
        switch (status) {
            case 400:
                toast.error(data);
                break;
            case 401:
                if (headers['www-authenticate']?.startsWith('Bearer error="invalid_token"')) {
                    store.userStore.logout();
                    toast.info('Session expired - please login again');
                } else {
                    toast.error('Error code 401: Unauthorized');
                }
                break;
            case 403:
                toast.error('Error code 403: Forbidden');
                break;
            case 404:
                toast.error('Error code 404: Not found');
                break;
            case 500:
                store.commonStore.setServerError(data);
                break;
        }
    }

    return Promise.reject(error);
});

const requests = {
    get: <T>(url: string) => axios.get<T>(url).then(responseBody),
    post: <T>(url: string, body: object) => axios.post<T>(url, body).then(responseBody),
    put: <T>(url: string, body: object) => axios.put<T>(url, body).then(responseBody),
    del: <T>(url: string) => axios.delete<T>(url).then(responseBody)
}

const AccountRequests = {
    current: () => requests.get<AuthUserDto>('/account'),
    login: (user: LoginRequestDto) => requests.post<LoginResponseDto>('/account/login', user),
    register: (user: RegisterRequestDto) => requests.post<AuthUserDto>('/account/register', user),
    refreshToken: () => requests.post<AuthUserDto>('/account/refreshToken', {})
}

const TodoItemsRequests = {
    getAll: () => requests.get<TodoItem[]>('/TodoItems'), 
    create: (todoItem: CreateTodoItem): Promise<TodoItem> => 
        requests.post<TodoItem>('/TodoItems', todoItem), 
    getById: (id: number) => requests.get<TodoItem>(`/TodoItems/${id}`),
    update: (id: number, todoItem: TodoItem) => requests.put<TodoItem>(`/TodoItems/${id}`, todoItem),
    delete: (id: number) => requests.del<void>(`/TodoItems/${id}`)
};

const agent = {
    AccountRequests,
    TodoItemsRequests
}
export default agent;
