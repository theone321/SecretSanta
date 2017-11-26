SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;


\connect santa


-- Add some accounts
INSERT INTO "Names"  ("RegisteredName") VALUES ('Michael Marvin');
INSERT INTO "Names" ("RegisteredName") VALUES ('Sarah Marvin-Foley');

SELECT * FROM "Names";