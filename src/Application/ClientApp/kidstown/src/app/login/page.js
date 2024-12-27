'use client'
import React, {Suspense} from 'react';
import dynamic from 'next/dynamic';

const Login = dynamic(() => import('../components/Login'), {ssr: false});

function LoginPage() {
    return (
        <Suspense fallback={<div>Loading...</div>}>
            <Login/>
        </Suspense>
    );
}

export default LoginPage;
