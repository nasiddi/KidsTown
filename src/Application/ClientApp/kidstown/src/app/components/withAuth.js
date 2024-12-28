'use client'
import React from 'react';
import {useAuth} from './AuthContext'; // Adjust the import path as needed
import {usePathname, useRouter} from 'next/navigation'; // Import the router hook from Next.js

function withAuth(WrappedComponent) {
    return function AuthComponent(props) {
        const {authenticated, loading} = useAuth();
        const pathname = usePathname(); // Get the current path
        const router = useRouter();

        React.useEffect(() => {
            if (!loading && !authenticated) {
                // Redirect to login with the current path as a query parameter
                router.push(`/login?redirect=${encodeURIComponent(pathname)}`);
            }
        }, [authenticated, loading, pathname]);

        if (loading) {
            return <div>Loading...</div>; // Or a spinner
        }

        if (!authenticated) {
            return <></>; // Render nothing while redirecting
        }

        return <WrappedComponent {...props} />;
    };
}

export default withAuth;
