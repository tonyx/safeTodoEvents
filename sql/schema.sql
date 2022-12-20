create user todo with password 'todo';

create database todo;
grant all privileges on database todo to todo;

\c todo;

--
-- PostgreSQL database dump
--

-- Dumped from database version 14.4
-- Dumped by pg_dump version 15.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: events; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.events (
    id integer NOT NULL,
    event json NOT NULL,
    "timestamp" timestamp without time zone NOT NULL
);


ALTER TABLE public.events OWNER TO postgres;

--
-- Name: events_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.events ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.events_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: eventsid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.eventsid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.eventsid_seq OWNER TO postgres;

--
-- Name: snapshots; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.snapshots (
    id integer DEFAULT nextval('public.eventsid_seq'::regclass) NOT NULL,
    snapshot json NOT NULL,
    event_id integer NOT NULL,
    "timestamp" timestamp without time zone NOT NULL
);


ALTER TABLE public.snapshots OWNER TO postgres;

--
-- Data for Name: events; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.events (id, event, "timestamp") FROM stdin;
\.


--
-- Data for Name: snapshots; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.snapshots (id, snapshot, event_id, "timestamp") FROM stdin;
\.


--
-- Name: events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.events_id_seq', 8408, true);


--
-- Name: eventsid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.eventsid_seq', 1038, true);


--
-- Name: events events_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.events
    ADD CONSTRAINT events_pkey PRIMARY KEY (id);


--
-- Name: snapshots snapshots_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshots
    ADD CONSTRAINT snapshots_pkey PRIMARY KEY (id);


--
-- Name: snapshots event_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshots
    ADD CONSTRAINT event_fk FOREIGN KEY (event_id) REFERENCES public.events(id) MATCH FULL ON DELETE CASCADE;


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- Name: TABLE events; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.events TO todo;


--
-- Name: SEQUENCE events_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.events_id_seq TO todo;


--
-- Name: SEQUENCE eventsid_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.eventsid_seq TO todo;


--
-- Name: TABLE snapshots; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.snapshots TO todo;


--
-- PostgreSQL database dump complete
--

