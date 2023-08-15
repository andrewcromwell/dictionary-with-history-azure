import React, { useState, useEffect } from "react";
import Navbar from "react-bootstrap/Navbar";
import "./App.css";
import Routes from "./Routes";
import Nav from "react-bootstrap/Nav";
import { LinkContainer } from "react-router-bootstrap";
import { AppContext } from "./lib/contextLib";
import { useNavigate } from "react-router-dom";
import { onError } from "./lib/errorLib";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";

function App() {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  async function handleLogout() {
    instance.logoutRedirect({
      postLogoutRedirectUri: "/",
    });
  }

  async function handleLogin() {
    instance.loginRedirect(loginRequest).catch(e => {
      console.log(e);
    });
  }

  return (
    <div className="App container py-3">
      <Navbar collapseOnSelect bg="light" expand="md" className="mb-3 px-3">
        <LinkContainer to="/">
          <Navbar.Brand className="fw-bold text-muted">Dictionary with history</Navbar.Brand>
        </LinkContainer>
        <Navbar.Toggle />
        <Navbar.Collapse className="justify-content-end">
          <Nav activeKey={window.location.pathname}>
            {isAuthenticated ? (
              <Nav.Link onClick={handleLogout}>Logout</Nav.Link>
            ) : (
              <Nav.Link onClick={handleLogin}>Login</Nav.Link>
            )}
          </Nav>
        </Navbar.Collapse>
      </Navbar>
      <AppContext.Provider value={{ isAuthenticated }}>
        <Routes />
      </AppContext.Provider>
    </div>
  );
}


export default App;
