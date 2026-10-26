# The Linux dedicated server of Match Please. The build context is the unpacked server build of the Build workflow
# (MatchPleaseServer.x86_64, UnityPlayer.so, MatchPleaseServer_Data).
FROM ubuntu:24.04

RUN apt-get update \
    && apt-get install --yes --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /opt/match-please
COPY . .

# The files stay root's: the server only reads them, and runs as the image's unprivileged user.
USER ubuntu
EXPOSE 7777/udp
ENTRYPOINT ["./MatchPleaseServer.x86_64", "-logFile", "-"]
CMD ["-server", "7777"]
