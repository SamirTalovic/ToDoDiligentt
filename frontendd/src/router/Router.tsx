import { RouteObject, createBrowserRouter } from "react-router-dom";
import App from "../App";
import RequireAuth from "./RequireAuth";
import LoginPage from "../pages/LoginPage";

export const routes: RouteObject[] = [
    {
        path: '/',
        element: <App />,
        children: [
            {
                element: <RequireAuth />,
                children: [
                ]
            },
            { path: '/login', element: <LoginPage /> }
        ]
    }
];

export const router = createBrowserRouter(routes);
