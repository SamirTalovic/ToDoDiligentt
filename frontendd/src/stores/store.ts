import { createContext, useContext } from "react";
import CommonStore from "./commonStore";
import UserStore from "./userStore";
import TodoItemStore from "./todoStore";

interface Store {
    commonStore: CommonStore;
    userStore: UserStore;
    todoStore: TodoItemStore
    
}

export const store: Store = {
    commonStore: new CommonStore(),
    userStore: new UserStore(),
    todoStore: new TodoItemStore()
   
}

export const StoreContext = createContext(store);

export function useStore() {
    return useContext(StoreContext);
}