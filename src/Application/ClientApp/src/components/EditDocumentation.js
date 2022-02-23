import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import classnames from 'classnames'
import { FullText, TextImageSplit, Title } from './DocElements'
import {
	Button,
	Card,
	Dropdown,
	DropdownItem,
	DropdownMenu,
	DropdownToggle,
	Form,
	FormGroup,
	Input,
	Label,
	Nav,
	Navbar,
} from 'reactstrap'
import { grey } from '@material-ui/core/colors'

function EditDocElement(props) {
	return (
		<Card key={props.id} style={{ padding: 15, margin: 20 }}>
			<Grid
				container
				spacing={1}
				alignItems="center"
				style={{
					display: 'flex',
					justifyContent: 'flex-end',
					color: 'red',
				}}
			>
				<Grid item>
					<Button id="up" name={props.id} onClick={props.onUp}>
						Up
					</Button>
				</Grid>
				<Grid item>
					<Button>Down</Button>
				</Grid>
			</Grid>

			{props.isTitle ? (
				<EditTitle size={props.size} text={props.title} />
			) : (
				<EditEntry
					title={props.title}
					fileName={props.fileName}
					paragraphs={props.paragraphs}
				/>
			)}
		</Card>
	)
}

function EditEntry(props) {
	const sortedParagraphs = _.sortBy(props.paragraphs, [
		function (p) {
			return p.position
		},
	])

	return (
		<Form>
			<FormGroup>
				<Label for="title">Title</Label>
				<Input
					value={props.title}
					type="text"
					name="title"
					id="title"
				/>
			</FormGroup>
			<FormGroup>
				<Label for="fileName">FileName</Label>
				<Input
					value={props.fileName}
					type="text"
					name="fileName"
					id="fileName"
				/>
			</FormGroup>
			{_.map(sortedParagraphs, (p) => (
				<EditParagraph
					onParagraphMove={props.onParagraphMove}
					paragraph={p}
				/>
			))}
		</Form>
	)
}

function EditParagraph(props) {
	return (
		<Card style={{ backgroundColor: '#b4d0fd', padding: 10, margin: 5 }}>
			<Grid
				container
				spacing={1}
				style={{
					display: 'flex',
					justifyContent: 'flex-start',
					color: 'red',
				}}
			>
				<Grid item>
					<Button
						id="up"
						name={props.paragraph.id}
						onClick={props.onParagraphMove}
					>
						Up
					</Button>
				</Grid>
				<Grid item>
					<Button
						id="down"
						name={props.paragraph.id}
						onClick={props.onParagraphMove}
					>
						Down
					</Button>
				</Grid>
			</Grid>
			<FormGroup>
				<Label for="icon">Icon</Label>
				<Input
					type="select"
					name={props.paragraph.id}
					id="icon"
					onChange={props.onIconSelectChange}
					value={props.icon}
				>
					<option>Action</option>
					<option>Info</option>
					<option>Warning</option>
				</Input>
			</FormGroup>
			<FormGroup>
				<Label for="text">Text</Label>
				<Input
					value={props.paragraph.text}
					type="textarea"
					name="fileName"
					id="fileName"
				/>
			</FormGroup>
		</Card>
	)
}

function EditTitle(props) {
	return (
		<Form>
			<Grid container spacing={1}>
				<Grid item sm={1}>
					<Label for="size">Size</Label>
				</Grid>
				<Grid item sm={11}>
					<Label for="title">Title</Label>
				</Grid>
				<Grid item sm={1}>
					<Input
						value={props.size}
						type="number"
						name="size"
						id="size"
						name={}
						onTextChange
					/>
				</Grid>
				<Grid item sm={11}>
					<Input
						value={props.text}
						type="text"
						name="title"
						id="title"
					/>
				</Grid>
			</Grid>
		</Form>
	)
}

export class EditDocumentation extends Component {
	static displayName = EditDocumentation.name
	repeat

	constructor(props) {
		super(props)

		this.state = {
			loading: false,
			content: [],
		}

		this.handleDropdownEvent = this.handleDropdownEvent.bind(this)
		this.toggleDropdown = this.toggleDropdown.bind(this)
		this.onUp = this.onUp.bind(this)
	}

	handleDropdownEvent(event) {
		const dropdown = event.target.name
		this.toggleDropdown(dropdown)
	}

	toggleDropdown(dropdown) {
		const previous = this.state[dropdown]
		this.setState({ [dropdown]: !previous })
	}

	onUp(event) {
		const doc = this.state.content
		const element = _.find(doc, function (d) {
			return d.id === parseInt(event.target.name, 10)
		})

		const upperElement = _.find(doc, function (d) {
			return (
				d.position === element.position - 1 && d.tabId === element.tabId
			)
		})

		element.position = element.position - 1
		upperElement.position = upperElement.position + 1

		this.setState({ content: doc })
	}

	onIconSelectChange(event) {
		console.log(event)
	}

	async componentDidMount() {
		const response = await fetch('documentation')
		const doc = await response.json()
		this.setState({ content: doc })
	}

	renderElement(element) {
		if (element.isTitle) {
			return <Title text={element.title.text} size={element.title.size} />
		}

		const entry = element.entry

		const sortedParagraphs = _.sortBy(entry.paragraphs, [
			function (p) {
				return p.position
			},
		])

		if (entry.fileName !== null) {
			return (
				<TextImageSplit
					title={entry.title !== null ? entry.title : ''}
					fileName={entry.fileName}
					paragraphs={sortedParagraphs}
				/>
			)
		}

		return <FullText paragraphs={sortedParagraphs} />
	}

	render() {
		if (this.state.loading) {
			return <div />
		}

		let checkInsApp = []

		if (this.state.content.length > 0) {
			checkInsApp = _(this.state.content)
				.filter((c) => c.entry !== null || c.title !== null)
				.filter((c) => c.tabId === 1)
				.sortBy((e) => e.position)
				.value()

			console.log(checkInsApp)
		}

		return _.map(checkInsApp, (e) => (
			<EditDocElement
				key={e.id}
				id={e.id}
				size={e.title?.size}
				title={e.title?.text ?? e.entry?.title}
				onUp={this.onUp}
				isTitle={e.isTitle}
				fileName={e.entry?.fileName}
				paragraphs={e.entry?.paragraphs}
			/>
		))
	}
}
