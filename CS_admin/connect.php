<?php

if (!function_exists('admin_base_path')) {
    function admin_base_path()
    {
        static $basePath = null;

        if ($basePath !== null) {
            return $basePath;
        }

        $scriptName = isset($_SERVER['SCRIPT_NAME']) ? str_replace('\\', '/', (string) $_SERVER['SCRIPT_NAME']) : '';
        $directory = str_replace('\\', '/', dirname($scriptName));

        if ($directory === '/' || $directory === '\\' || $directory === '.') {
            $basePath = '';
            return $basePath;
        }

        $basePath = rtrim($directory, '/');
        return $basePath;
    }
}

if (!function_exists('admin_url')) {
    function admin_url($path = '')
    {
        $path = ltrim((string) $path, '/');
        $basePath = admin_base_path();

        if ($path === '') {
            return $basePath !== '' ? $basePath : '/';
        }

        return ($basePath !== '' ? $basePath : '') . '/' . $path;
    }
}

if (!function_exists('backend_base_url')) {
    function backend_base_url()
    {
        static $baseUrl = null;

        if ($baseUrl !== null) {
            return $baseUrl;
        }

        $configuredUrl = getenv('CSA_BACKEND_BASE_URL');
        if (is_string($configuredUrl) && trim($configuredUrl) !== '') {
            $baseUrl = rtrim(trim($configuredUrl), '/');
            return $baseUrl;
        }

        $baseUrl = 'https://localhost:7123';
        return $baseUrl;
    }
}

if (!function_exists('backend_api_url')) {
    function backend_api_url($path = '')
    {
        return backend_base_url() . '/api/' . ltrim((string) $path, '/');
    }
}

if (!function_exists('backend_public_url')) {
    function backend_public_url($path = '')
    {
        return backend_base_url() . '/' . ltrim((string) $path, '/');
    }
}
