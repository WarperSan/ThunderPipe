# CI-only replacement for Thunderstore 0.187.0/minio/Dockerfile.
# Both images were successfully pulled via this mirror in the local setup.
# Copy the version-matched mc from the image instead of wget from dl.min.io.
FROM swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/minio/mc:RELEASE.2021-06-13T17-48-22Z AS mc
FROM swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/minio/minio:RELEASE.2021-06-17T00-10-46Z

RUN microdnf install netcat
COPY --from=mc /usr/bin/mc /usr/bin/mc
COPY ./minio/thunderstore-entrypoint.sh /usr/bin/thunderstore-entrypoint.sh
RUN chmod +x /usr/bin/thunderstore-entrypoint.sh

EXPOSE 9000
ENTRYPOINT ["/usr/bin/thunderstore-entrypoint.sh"]
VOLUME ["/data"]
CMD ["minio"]
