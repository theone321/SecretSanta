SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;


\connect santa


-- Settings
INSERT INTO "Settings" VALUES ('SessionTimeout', '50000', 0); --about 34 days

--EventTypes
INSERT INTO "EventTypes" VALUES (1, 'Secret Match');
INSERT INTO "EventTypes" VALUES (2, 'Birthday Party');
