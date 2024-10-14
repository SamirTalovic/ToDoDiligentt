import { create } from "zustand";
import { User } from "../common/interfaces/UserInterface";
import { LoginRequestDto } from "../common/interfaces/AuthInterface";
import agent from "../api/agent";
import { router } from "../router/Router";

interface UserStoreState {
    user: User | null;
    loading: boolean;
    isLoggedIn: () => boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    getUser: () => Promise<void>;
}

export const useUserStore = create<UserStoreState>((set, get) => ({
    user: null,
    loading: false,

    isLoggedIn: () => !!get().user,

    login: async (email: string, password: string) => {
        set({ loading: true });
        try {
            const loginRequest: LoginRequestDto = { email, password };
            const response = await agent.AccountRequests.login(loginRequest); 
            const user: User = {
                id: response.id,
                name: response.name,
                email: response.email,
            };
            set({ user });
            await get().getUser(); 
            router.navigate("/");
        } catch (error) {
            console.error(error);
        } finally {
            set({ loading: false });
        }
    },

    logout: () => {
        set({ user: null });
        router.navigate("/"); 
    },

    getUser: async () => {
        set({ loading: true });
        try {
            const response = await agent.AccountRequests.current();
            const user: User = {
                id: response.id,
                name: response.name,
                email: response.email,
            };
            set({ user });
        } catch (error) {
            console.error(error);
        } finally {
            set({ loading: false });
        }
    },
}));
