--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.1
-- Dumped by pg_dump version 9.6.1

-- Started on 2017-01-27 10:44:09

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 2148 (class 1262 OID 24576)
-- Name: santa; Type: DATABASE; Schema: -; Owner: santa
--

CREATE DATABASE santa WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'en_US.utf8' LC_CTYPE = 'en_US.utf8';


ALTER DATABASE santa OWNER TO santa;

\connect santa

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 1 (class 3079 OID 12393)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2150 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 189 (class 1259 OID 24595)
-- Name: Name; Type: TABLE; Schema: public; Owner: santa
--

CREATE TABLE "Name" (
    "Id" int NOT NULL,
    "RegisteredName" text,
    "Password" text,
    "HasRegistered" bool DEFAULT FALSE
);


ALTER TABLE "Name" OWNER TO santa;

--
-- TOC entry 188 (class 1259 OID 24593)
-- Name: Name_Id_seq; Type: SEQUENCE; Schema: public; Owner: santa
--

CREATE SEQUENCE "Name_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "Name_Id_seq" OWNER TO santa;

--
-- TOC entry 2151 (class 0 OID 0)
-- Dependencies: 188
-- Name: Name_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: santa
--

ALTER SEQUENCE "Name_Id_seq" OWNED BY "Name"."Id";

--
-- TOC entry 2018 (class 2604 OID 24598)
-- Name: Name Id; Type: DEFAULT; Schema: public; Owner: santa
--

ALTER TABLE ONLY "Name" ALTER COLUMN "Id" SET DEFAULT nextval('"Name_Id_seq"'::regclass);

--\
--
-- TOC entry 2025 (class 2606 OID 24603)
-- Name: Name PK_Name; Type: CONSTRAINT; Schema: public; Owner: santa
--

ALTER TABLE ONLY "Name"
    ADD CONSTRAINT "PK_Name" PRIMARY KEY ("Id");

-- Completed on 2017-01-27 10:44:09

--
-- PostgreSQL database dump complete
--
