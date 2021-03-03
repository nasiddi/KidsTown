import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
 
export class CheckInLayout extends Component {
  static displayName = CheckInLayout.name;

  render () {
    return (
      <div>
        <NavMenu />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}

export class OverviewLayout extends Component {
    static displayName = CheckInLayout.name;

    render () {
        return (
            <div>
                <Container className="themed-container" fluid={true}>
                    {this.props.children}
                </Container>
            </div>
        );
    }
}

