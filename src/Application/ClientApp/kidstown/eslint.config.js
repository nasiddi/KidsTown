// ESLint v9+ flat config
import js from '@eslint/js'
import tseslint from '@typescript-eslint/eslint-plugin'
import tsParser from '@typescript-eslint/parser'
import prettierConfig from 'eslint-config-prettier'
import importPlugin from 'eslint-plugin-import'
import jsxA11y from 'eslint-plugin-jsx-a11y'
import prettier from 'eslint-plugin-prettier'
import react from 'eslint-plugin-react'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import unusedImports from 'eslint-plugin-unused-imports'
import globals from 'globals'

export default [
  {
    ignores: ['dist/**', 'node_modules/**'],
  },

  js.configs.recommended,

  // Main rules
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    languageOptions: {
      parser: tsParser,
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module',
        ecmaFeatures: { jsx: true },
      },
      globals: {
        ...globals.browser,
        ...globals.node,
      },
    },
    settings: {
      react: { version: 'detect' },
      'import/resolver': {
        typescript: true,
        node: true,
      },
    },
    plugins: {
      react,
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      import: importPlugin,
      'jsx-a11y': jsxA11y,
      '@typescript-eslint': tseslint,
      'unused-imports': unusedImports,
      prettier, // ✅ add plugin so prettier/prettier rule works
    },
    rules: {
      curly: ['error', 'all'],
      'react/react-in-jsx-scope': 'off',

      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],

      'import/order': [
        'warn',
        {
          'newlines-between': 'always',
          alphabetize: { order: 'asc', caseInsensitive: true },
        },
      ],

      'unused-imports/no-unused-imports': 'error',
      'unused-imports/no-unused-vars': [
        'warn',
        {
          vars: 'all',
          varsIgnorePattern: '^_',
          args: 'after-used',
          argsIgnorePattern: '^_',
        },
      ],
      'no-unused-vars': 'off',

      '@typescript-eslint/consistent-type-imports': ['warn', { prefer: 'type-imports' }],
      '@typescript-eslint/no-unused-vars': 'off',
    },
  },

  // ✅ Prettier last, so it wins
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: { prettier },
    rules: {
      ...prettierConfig.rules, // disables conflicting formatting rules
      'prettier/prettier': 'error', // enforces prettier formatting as ESLint errors
    },
  },
]
