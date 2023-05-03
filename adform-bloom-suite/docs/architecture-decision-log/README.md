# Architecture Decision Log

It is suggested to install ADR tools as described [here](https://github.com/npryce/adr-tools/blob/master/INSTALL.md).

It is also worth reading [this](http://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions) and [this](https://github.com/joelparkerhenderson/architecture_decision_record).

Then you can create a new Architecture Decision Record:

```
adr new NAME_OF_NEW_RECORD
```

Supercede existing one:

```
adr new -s NAME_OD_RECORD_TO_SUPERCEDE
```

Create a new record that is linked to another one:

```
adr new -l "NAME_OF_EXISTING_RECORD" NAME_OF_NEW_RECORD
```

Generate table of contents:

```
adr generate toc
```

Or a graph of records:

```
adr generate graph
```
