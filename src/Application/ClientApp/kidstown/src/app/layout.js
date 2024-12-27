import "./globals.css";
import {AuthProvider} from "@/app/components/AuthContext";

export const metadata = {
    title: "KidsTown",
    icons: {
        icon: "https://cdn3.iconfinder.com/data/icons/education-209/64/plane-paper-toy-science-school-512.png"
    }
};

export default function RootLayout({children}) {
    return (
        <html lang="en">
        <body>
        <AuthProvider>
            {children}
        </AuthProvider>
        </body>
        </html>
    );
}
