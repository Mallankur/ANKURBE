# 4. use-separate-projects-for-bloom-and-bloom-runtime

Date: 2019-11

## Status

Accepted

## Context

Bloom solution consists of 2 kinds of endpoints i.e.: endpoints that allow one to manage roles, assignments etc. and endpoints that allow one to perform evaluation of a given user.
From the performance perspective evaluation endpoints are crucial.

## Decision

It was decided to have 2 projects/APIs i.e. Bloom and Bloom Runtime. 

## Consequences

We will be able to scale Bloom Runtime in the independent way to Bloom.
