SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;


\connect santa


-- Users
INSERT INTO "Users"  ("UserName", "RegisteredName", "Password", "IsAdmin") VALUES ('magico13', 'Michael Marvin', 'password', TRUE);

SELECT * FROM "Users";
    
-- Settings
INSERT INTO "Settings" VALUES ('AllowRegistration', 'true');
INSERT INTO "Settings" VALUES ('AllowMatching', 'false');
INSERT INTO "Settings" VALUES ('SessionTimeout', '50000'); --about 34 days