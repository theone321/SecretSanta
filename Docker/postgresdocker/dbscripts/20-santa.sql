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

-- Create Users

CREATE TABLE "Users" (
    "Id" int NOT NULL,
	"UserName" text NOT NULL,
    "RegisteredName" text,
    "Password" text,
    "IsSuperAdmin" bool DEFAULT FALSE,
    "Interests" text
);

ALTER TABLE "Users" OWNER TO santa;

CREATE SEQUENCE "Users_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER TABLE "Users_Id_seq" OWNER TO santa;

ALTER SEQUENCE "Users_Id_seq" OWNED BY "Users"."Id";

ALTER TABLE ONLY "Users" ALTER COLUMN "Id" SET DEFAULT nextval('"Users_Id_seq"'::regclass);

ALTER TABLE ONLY "Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");

-- Create Matches

CREATE TABLE "Matches" (
    "Id" int NOT NULL,
	"EventId" int NOT NULL,
    "RequestorId" int,
    "MatchedId" int,
    "RerollAllowed" bool DEFAULT TRUE
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
	"EventId" int,
    "RequestorId" int,
    "RestrictedId" int,
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
CREATE TABLE "Settings" (
    "Name" text NOT NULL,
    "Value" text,
	"EventId" int
);

ALTER TABLE "MatchRestrictions" OWNER TO santa;

-- Create Sessions
CREATE TABLE "Sessions" (
    "SessionId" text NOT NULL,
    "User" text NOT NULL,
    "TimeStamp" timestamp NOT NULL,
	"EventId" int
);

-- Create Events
CREATE TABLE "Events" (
	"Id" int NOT NULL,
	"Name" text NOT NULL,
	"StartDate" timestamp NOT NULL,
	"Location" text,
	"Description" text,
	"SharedId" text NOT NULL,
	"EventType" int NOT NULL,
	"BirthdayPersonUserId" int,
	"IsSurpriseEvent" boolean NOT NULL
);

ALTER TABLE "Events" OWNER TO santa;

CREATE SEQUENCE "Events_Id_seq";

ALTER TABLE "Events_Id_seq" OWNER TO santa;

ALTER SEQUENCE "Events_Id_seq" OWNED BY "Events"."Id";

ALTER TABLE ONLY "Events" ALTER COLUMN "Id" SET DEFAULT nextval('"Events_Id_seq"'::regclass);

ALTER TABLE ONLY "Events"
    ADD CONSTRAINT "PK_Events" PRIMARY KEY ("Id");
	
-- Create EventAdmins
CREATE TABLE "EventAdmins" (
	"Id" int NOT NULL,
	"EventId" int NOT NULL,
	"AdminId" int NOT NULL
);

ALTER TABLE "EventAdmins" OWNER TO santa;

CREATE SEQUENCE "EventAdmins_Id_seq";

ALTER TABLE "EventAdmins_Id_seq" OWNER TO santa;

ALTER SEQUENCE "EventAdmins_Id_seq" OWNED BY "EventAdmins"."Id";

ALTER TABLE ONLY "EventAdmins" ALTER COLUMN "Id" SET DEFAULT nextval('"EventAdmins_Id_seq"'::regclass);

ALTER TABLE ONLY "EventAdmins"
    ADD CONSTRAINT "PK_EventAdmins" PRIMARY KEY ("Id");
	
-- Create UserEvents
CREATE TABLE "UserEvents" (
	"Id" int NOT NULL,
	"UserId" int NOT NULL,
	"EventId" int NOT NULL
);

ALTER TABLE "UserEvents" OWNER TO santa;

CREATE SEQUENCE "UserEvents_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER TABLE "UserEvents_Id_seq" OWNER TO santa;

ALTER SEQUENCE "UserEvents_Id_seq" OWNED BY "UserEvents"."Id";

ALTER TABLE ONLY "UserEvents" ALTER COLUMN "Id" SET DEFAULT nextval('"UserEvents_Id_seq"'::regclass);

ALTER TABLE ONLY "UserEvents"
    ADD CONSTRAINT "PK_UserEvents" PRIMARY KEY ("Id");
	
-- Create EventTypes
CREATE TABLE "EventTypes" (
	"Id" int NOT NULL,
	"Name" text NOT NULL
);

ALTER TABLE "EventTypes" OWNER TO santa;

CREATE SEQUENCE "EventTypes_Id_seq";

ALTER TABLE "EventTypes_Id_seq" OWNER TO santa;

ALTER SEQUENCE "EventTypes_Id_seq" OWNED BY "EventTypes"."Id";

ALTER TABLE ONLY "EventTypes" ALTER COLUMN "Id" SET DEFAULT nextval('"EventTypes_Id_seq"'::regclass);

ALTER TABLE ONLY "EventTypes"
    ADD CONSTRAINT "PK_EventTypes" PRIMARY KEY ("Id");
	
-- Create EventItems
CREATE TABLE "EventItems" (
	"Id" int NOT NULL,
	"EventId" int NOT NULL,
	"ItemText" text NOT NULL,
	"IsGiftIdea" boolean NOT NULL,
	"IsBroughtItem" boolean NOT NULL,
	"UserIdBringingItem" int
);

ALTER TABLE "EventItems" OWNER TO santa;

CREATE SEQUENCE "EventItems_Id_seq";

ALTER TABLE "EventItems_Id_seq" OWNER TO santa;

ALTER SEQUENCE "EventItems_Id_seq" OWNED BY "EventItems"."Id";

ALTER TABLE ONLY "EventItems" ALTER COLUMN "Id" SET DEFAULT nextval('"EventItems_Id_seq"'::regclass);

ALTER TABLE ONLY "EventItems"
    ADD CONSTRAINT "PK_EventItems" PRIMARY KEY ("Id");
