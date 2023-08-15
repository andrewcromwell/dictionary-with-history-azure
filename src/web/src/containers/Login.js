import React, { useState } from "react";
import Form from "react-bootstrap/Form";
import LoaderButton from "../components/LoaderButton";
import { useAppContext } from "../lib/contextLib";
import { useFormFields } from "../lib/hooksLib";
import { onError } from "../lib/errorLib";
import "./Login.css";
import { loginRequest } from "../authConfig";
import { useMsal } from "@azure/msal-react";

export default function Login() {
    const { instance } = useMsal();
    const [isLoading, setIsLoading] = useState(false);
    const [fields, handleFieldChange] = useFormFields({
      username: "",
      password: "",
    });

  function validateForm() {
    return fields.username.length > 0 && fields.password.length > 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setIsLoading(true);

    try {
      instance.loginRedirect(loginRequest).catch(e => {
        console.log(e);
      });
    } catch (e) {
      onError(e);
      setIsLoading(false);
    }
  }

  return (
    <div className="Login">
      <Form onSubmit={handleSubmit}>
        <Form.Group size="lg" controlId="username">
          <Form.Label>Username</Form.Label>
          <Form.Control
            autoFocus
            type="text"
            value={fields.username}
            onChange={handleFieldChange}
          />
        </Form.Group>
        <Form.Group size="lg" controlId="password">
          <Form.Label>Password</Form.Label>
          <Form.Control
            type="password"
            value={fields.password}
            onChange={handleFieldChange}
          />
        </Form.Group>
        <div className="d-grid gap-2 mt-3">
            <LoaderButton
                block="true"
                size="lg"
                type="submit"
                isLoading={isLoading}
                disabled={!validateForm()}
            >
                Login
            </LoaderButton>
        </div>
      </Form>
    </div>
  );
}