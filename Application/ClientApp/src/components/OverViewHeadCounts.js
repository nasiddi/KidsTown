import React, { Component } from 'react';
import { Grid } from "@material-ui/core";
import {
    fetchLocations,
    getSelectedEventFromStorage,
    getStringFromSession,
    getSelectedOptionsFromStorage, getLastSunday,
} from "./Common";
import { Table } from 'reactstrap';
import {withAuth} from "../auth/MsalAuthProvider";

class OverViewHeadCounts extends Component {
    static displayName = OverViewHeadCounts.name;
    repeat;


    constructor (props) {
        super(props);
        
        this.state = {
            headCounts: [],
            loading: true
        };
    }

    async componentDidMount() {
        const locations = await fetchLocations();
        this.setState({locations: locations})
        await this.fetchData();
        this.setState({ loading: false})
    }

    componentWillUnmount() {
        clearTimeout(this.repeat);
    }
    
    renderCounts(){
        if (this.state.loading){
            return (
                <div/>
            );
        }
        
        
        let totalCount = <tr/>;
        if (getSelectedOptionsFromStorage('overviewLocations', []).length !== 1){
            totalCount = 
                <tr key='Total'>
                <th>Total</th>
                <th align="right">{this.GetTotalCount(false)}</th>
                <th align="right">{this.GetTotalCount(true)}</th>
            </tr>
        }
        
        
        return (
            <Table>
                <thead>
                <tr>
                    <th>Location</th>
                    <th>Kinder</th>
                    <th>Betreuer</th>
                </tr>
                </thead>
                <tbody>
                {getSelectedOptionsFromStorage('overviewLocations', []).map((row) => (
                    <tr key={row.value}>
                        <td> {row.label}</td>
                        <td>{this.GetCount(row.value, false)}</td>
                        <td>{this.GetCount(row.value, true)}</td>
                    </tr>
                ))}
                {totalCount}
                </tbody>
            </Table>
        );
        }
        
    GetCount(locationId, isVolunteer) {
        let headCounts = this.state.headCounts.find(c => c['locationId'] === locationId);
        if (headCounts === undefined){
            return 0;
        }

        if (isVolunteer){
            return headCounts['volunteerCount']
        }
        return headCounts['regularCount'] + headCounts['guestCount'];
    }

    GetTotalCount(isVolunteer) {
        if (isVolunteer){
            let locationCount = this.state.headCounts.map(h => h['volunteerCount'])
            return locationCount.reduce(this.sum, 0);
        }
        
        let locationCount = this.state.headCounts.map(h => h['regularCount'] + h['guestCount'])
        return locationCount.reduce(this.sum, 0)
    }

    render () {
        if (this.state.loading){
            return (
                <div/>
            );
        }

        return (
            <div>
                <Grid container spacing={3} justify="space-between" alignItems="flex-start">
                    <Grid item xs={12}>
                        {this.renderCounts()}
                    </Grid>
                </Grid>
            </div>
        )
    }

    async fetchData() {
        await fetch(`overview/event/${await getSelectedEventFromStorage()}/attendees/headcounts?date=${getStringFromSession('overviewDate', getLastSunday().toISOString())}`, {
            body: JSON.stringify(getSelectedOptionsFromStorage('overviewLocations', []).map(l => l.value)),
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then((r) => r.json())
            .then((j) => {
                this.setState({ headCounts: j });
            });
        
        this.repeat = setTimeout(this.fetchData.bind(this), 500);
    }

    sum = (a, b) => {
        return a + b;
    }
}

export const OverviewHeadCount = withAuth(OverViewHeadCounts);
