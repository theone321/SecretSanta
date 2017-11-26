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

-- Create Names

CREATE TABLE "Names" (
    "Id" int NOT NULL,
    "RegisteredName" text,
    "Password" text,
    "HasRegistered" bool DEFAULT FALSE
);

ALTER TABLE "Names" OWNER TO santa;

CREATE SEQUENCE "Names_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER TABLE "Names_Id_seq" OWNER TO santa;

ALTER SEQUENCE "Names_Id_seq" OWNED BY "Names"."Id";

ALTER TABLE ONLY "Names" ALTER COLUMN "Id" SET DEFAULT nextval('"Names_Id_seq"'::regclass);

ALTER TABLE ONLY "Names"
    ADD CONSTRAINT "PK_Names" PRIMARY KEY ("Id");

-- Create Matches

CREATE TABLE "Matches" (
    "Id" int NOT NULL,
    "RequestorName" text,
    "MatchedName" text,
    "RerollAllowed" bool DEFAULT TRUE,
    "IsAdmin" bool DEFAULT FALSE,
    "Interests" text
);

ALTER TABLE "Matches" OWNER TO santa;

CREATE SEQUENCE "Matches_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER TABLE "Matches_Id_seq" OWNER TO santa;

ALTER SEQUENCE "Matches_Id_seq" OWNED BY "Matches"."Id";

ALTER TABLE ONLY "Matches" ALTER COLUMN "Id" SET DEFAULT nextval('"Matches_Id_seq"'::regclass);

ALTER TABLE ONLY "Matches"
    ADD CONSTRAINT "PK_Matches" PRIMARY KEY ("Id");
    
    
-- Create MatchRestrictions
CREATE TABLE "MatchRestrictions" (
    "Id" int NOT NULL,
    "RequestorName" text,
    "RestrictedName" text,
    "StrictRestriction" bool DEFAULT TRUE
);

ALTER TABLE "MatchRestrictions" OWNER TO santa;

CREATE SEQUENCE "MatchRestrictions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER TABLE "MatchRestrictions_Id_seq" OWNER TO santa;

ALTER SEQUENCE "MatchRestrictions_Id_seq" OWNED BY "MatchRestrictions"."Id";

ALTER TABLE ONLY "MatchRestrictions" ALTER COLUMN "Id" SET DEFAULT nextval('"MatchRestrictions_Id_seq"'::regclass);

ALTER TABLE ONLY "MatchRestrictions"
    ADD CONSTRAINT "PK_MatchRestrictions" PRIMARY KEY ("Id");

    
-- Create Settings
CREATE TABLE "Settings" {
    "Name" text NOT NULL,
    "Value" text
};

ALTER TABLE "MatchRestrictions" OWNER TO santa;