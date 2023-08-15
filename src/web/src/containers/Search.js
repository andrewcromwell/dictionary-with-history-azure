import React, { useRef, useState } from "react";
import Form from "react-bootstrap/Form";
import LoaderButton from "../components/LoaderButton";
import { useFormFields } from "../lib/hooksLib";
import { onError } from "../lib/errorLib";
import "./Search.css";
import ListGroup from "react-bootstrap/ListGroup";
import axios from "axios";
import config from "../config";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

export default function Search() {
  const { instance, accounts } = useMsal();
  const [fields, handleFieldChange] = useFormFields({
    word: "",
    media: "",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [searchResponse, setSearchResponse] = useState(null);

  function validateForm() {
    return fields.word.length > 0 && fields.media.length > 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setIsLoading(true);

    try {
      const searchResponse = await makePostSearchRequest(fields.word, fields.media);
    } catch (e) {
      onError(e);
    }
  }

  function makePostSearchRequest(searchTerm, media){
    instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0]
    }).then((response) => {
        postSearch(response.accessToken, searchTerm, media);
    });
  }

  function postSearch(accessToken, searchTerm, media) {
    const body = {
        word: searchTerm,
        media: media,
    };

    const requestConfig = {
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    };

    var baseURL = config.api.baseUrl;
    var endpoint = "/lookupWord";
    axios
      .post(baseURL + endpoint, body, requestConfig)
      .then(res => {
        const searchResponse = res.data;
        setSearchResponse(searchResponse);
        fields.word = "";
        fields.media = "";
        setIsLoading(false);
      });
  }

  function renderSearchPage() {
    return (
        <div className="Search">
          <Form onSubmit={handleSubmit}>
            <Form.Group controlId="word">
              <Form.Label>Search Term</Form.Label>
              <Form.Control
                value={fields.word}
                type="text"
                as="input"
                onChange={handleFieldChange}
              />
            </Form.Group>
            <Form.Group controlId="media">
              <Form.Label>Media Content</Form.Label>
              <Form.Control
                value={fields.media}
                type="text"
                as="input"
                onChange={handleFieldChange}
              />
            </Form.Group>
            <div className="d-grid gap-2 mt-3">
                <LoaderButton
                    block
                    type="submit"
                    size="lg"
                    variant="primary"
                    isLoading={isLoading}
                    disabled={!validateForm()}
                >
                    Search
                </LoaderButton>
            </div>
          </Form>
        </div>
      );
  }

  function showDefinition(word, definition) {
    if (definition.error){
      return (
        <>
          <span className="text-muted">{definition.error}</span>
        </>
      );
    }
    return (
      <>
      <p class="h2">{word}</p>
        {searchResponse.definition.definitions.map(({ partOfSpeech, meanings }) => (
          <>
            <span className="fw-bold">{partOfSpeech.trim()}</span>
            <br />
            <ol>
              {meanings.map(meaning => (
                <li>
                  <span className="text-muted">
                  {meaning.trim()}
                  </span>
                </li>
              ))}
            </ol>
          </>
        ))}
      </>
    );
  }
  function renderSearchResponse() {
    const definitionDisplay = showDefinition(searchResponse.word, searchResponse.definition);
    const searchHistory = searchResponse.lookupInfo;
    return (
        <div className="SearchHistory">
            {definitionDisplay}
            <ListGroup.Item action className="py-3 text-nowrap text-truncate">
                <a href={"https://en.wiktionary.org/wiki/" + searchResponse.word + "#German"} className="ms-2 fw-bold">Wiktionary</a>
            </ListGroup.Item>
            <ListGroup.Item action className="py-3 text-nowrap text-truncate">
                <span className="ms-2 fw-bold">You've searched this word {searchHistory.numberOfLookups} time(s)</span>
            </ListGroup.Item>
            {searchHistory.lookups.map(({ lookupDate, media }) => (
                <ListGroup.Item action className="text-nowrap text-truncate">
                    <span className="text-muted">Media Content: {media.trim()}</span>
                    <br />
                    <span className="text-muted">
                    Searched on: {new Date(lookupDate).toLocaleString()}
                    </span>
                </ListGroup.Item>
            ))}
        </div>
      );
  }

  return (
    <>
    <div id="Search">
      {renderSearchPage()}
    </div>
    <div id="Results">
      { searchResponse ? renderSearchResponse() : <></>}
    </div>
    </>
  );
}
