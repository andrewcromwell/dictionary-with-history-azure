import React from "react";
import { Route, Routes } from "react-router-dom";
import Home from "./containers/Home";
import Login from "./containers/Login";
import NotFound from "./containers/NotFound";
import Search from "./containers/Search";
import AuthenticatedRoute from "./components/AuthenticatedRoute";
import UnauthenticatedRoute from "./components/UnauthenticatedRoute";

export default function Links() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <UnauthenticatedRoute>
            <Home />
          </UnauthenticatedRoute>
        }
      />
      <Route
        path="/login"
        element={
          <UnauthenticatedRoute>
            <Login />
          </UnauthenticatedRoute>
        }
      />
      <Route
        path="/search"
        element={
          <AuthenticatedRoute>
            <Search />
          </AuthenticatedRoute>
        }
      />
      {
      /* Finally, catch all unmatched routes */
      }
      <Route path="*" element={<NotFound />} />;
    </Routes>
  );
}