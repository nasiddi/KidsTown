import 'bootstrap/dist/css/bootstrap.css'
import React from 'react'
import ReactDOM from 'react-dom'
import { BrowserRouter } from 'react-router-dom'
import { App } from './App'

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href')
const rootElement = document.getElementById('root')
document.title = 'Kidstown'

ReactDOM.render(
	<BrowserRouter basename={baseUrl}>
		<App />
	</BrowserRouter>,
	rootElement
)

registerServiceWorker()
